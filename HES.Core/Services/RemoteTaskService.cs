using HES.Core.Entities;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteTaskService : IRemoteTaskService
    {
        readonly ConcurrentDictionary<string, string> _devicesInProgress
            = new ConcurrentDictionary<string, string>();
        readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;
        readonly IAsyncRepository<Device> _deviceRepository;
        readonly IWorkstationProximityDeviceService _workstationProximityDeviceService;
        readonly ILogger<RemoteTaskService> _logger;
        readonly IDataProtectionService _dataProtectionService;
        readonly IDeviceAccessProfilesService _deviceAccessProfilesService;
        readonly IHubContext<EmployeeDetailsHub> _hubContext;

        public RemoteTaskService(IAsyncRepository<DeviceAccount> deviceAccountRepository,
                                 IAsyncRepository<DeviceTask> deviceTaskRepository,
                                 IAsyncRepository<Device> deviceRepository,
                                 IWorkstationProximityDeviceService workstationProximityDeviceService,
                                 ILogger<RemoteTaskService> logger,
                                 IDataProtectionService dataProtectionService,
                                 IDeviceAccessProfilesService deviceAccessProfilesService,
                                 IHubContext<EmployeeDetailsHub> hubContext)
        {
            _deviceAccountRepository = deviceAccountRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _deviceRepository = deviceRepository;
            _workstationProximityDeviceService = workstationProximityDeviceService;
            _logger = logger;
            _dataProtectionService = dataProtectionService;
            _deviceAccessProfilesService = deviceAccessProfilesService;
            _hubContext = hubContext;
        }

        public async Task AddTaskAsync(DeviceTask deviceTask)
        {
            await _deviceTaskRepository.AddAsync(deviceTask);
        }

        public async Task AddRangeAsync(IList<DeviceTask> deviceTasks)
        {
            await _deviceTaskRepository.AddRangeAsync(deviceTasks);
        }

        public async Task UndoLastTaskAsync(string accountId)
        {
            // Get account
            var deviceAccount = await _deviceAccountRepository.GetByIdAsync(accountId);

            // Get undo task
            var undoTask = await _deviceTaskRepository.Query()
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(t => t.DeviceAccountId == accountId);

            // Delete Task
            await _deviceTaskRepository.DeleteAsync(undoTask);

            // Get last task
            var lastTask = _deviceTaskRepository.Query()
                .Where(d => d.DeviceAccountId == deviceAccount.Id)
                .OrderByDescending(d => d.CreatedAt).FirstOrDefault();

            if (lastTask != null)
            {
                // Update account
                switch (lastTask.Operation)
                {
                    case TaskOperation.Create:
                        deviceAccount.Status = AccountStatus.Creating;
                        deviceAccount.UpdatedAt = null;
                        break;
                    case TaskOperation.Update:
                        deviceAccount.Status = AccountStatus.Updating;
                        deviceAccount.UpdatedAt = DateTime.UtcNow;
                        break;
                    case TaskOperation.Delete:
                        deviceAccount.Status = AccountStatus.Removing;
                        deviceAccount.UpdatedAt = DateTime.UtcNow;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                deviceAccount.Status = AccountStatus.Done;
                deviceAccount.UpdatedAt = DateTime.UtcNow;
            }

            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, new string[] { "Status", "UpdatedAt" });
            _logger.LogInformation("Undo task was successful.");
        }

        public async Task RemoveDeviceAsync(Device device)
        {
            await AddTaskAsync(new DeviceTask
            {
                Password = _dataProtectionService.Protect(device.MasterPassword),
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Wipe,
                DeviceId = device.Id
            });
        }

        public async Task ProcessTasksAsync(string deviceId)
        {
            Debug.WriteLine($"!!!!!!!!!!!!! ProcessTasksAsync {deviceId}");

            if (_devicesInProgress.TryAdd(deviceId, deviceId))
            {
                try
                {
                    Debug.WriteLine($"!!!!!!!!!!!!! ExecuteRemoteTasks start {deviceId}");
                    await ExecuteRemoteTasks(deviceId).TimeoutAfter(300_000);
                }
                catch (TimeoutException ex)
                {
                    Debug.Assert(false);
                    _logger.LogCritical(ex.Message, deviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                finally
                {
                    Debug.WriteLine($"!!!!!!!!!!!!! ExecuteRemoteTasks end {deviceId}");
                    _devicesInProgress.TryRemove(deviceId, out string removed);
                }
            }
            else
            {
                Debug.WriteLine($"!!!!!!!!!!!!! in progress {deviceId}");
            }
        }

        public void StartTaskProcessing(IList<string> deviceId)
        {
            foreach (var item in deviceId)
            {
                StartTaskProcessing(item);
            }
        }

        public void StartTaskProcessing(string deviceId)
        {
            Debug.WriteLine($"!!!!!!!!!!!!! StartTaskProcessing {deviceId}");

            if (_devicesInProgress.TryAdd(deviceId, deviceId))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        Debug.WriteLine($"!!!!!!!!!!!!! ExecuteRemoteTasks start {deviceId}");
                        await ExecuteRemoteTasks(deviceId).TimeoutAfter(300_000);
                    }
                    catch (TimeoutException ex)
                    {
                        Debug.Assert(false);
                        _logger.LogCritical(ex.Message, deviceId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    finally
                    {
                        Debug.WriteLine($"!!!!!!!!!!!!! ExecuteRemoteTasks end {deviceId}");
                        _devicesInProgress.TryRemove(deviceId, out string removed);
                    }
                });
            }
            else
            {
                Debug.WriteLine($"!!!!!!!!!!!!! in progress {deviceId}");
            }
        }

        private async Task<bool> ExecuteRemoteTasks(string deviceId)
        {
            try
            {
                _dataProtectionService.Validate();

                var tasks = await _deviceTaskRepository.Query()
                    .Include(t => t.DeviceAccount)
                    .Where(t => t.DeviceId == deviceId)
                    .OrderBy(x => x.CreatedAt)
                    .ToListAsync();

                while (tasks.Any())
                {
                    var remoteDevice = await AppHub.EstablishRemoteConnection(deviceId, 4);
                    if (remoteDevice == null)
                        break;

                    foreach (var task in tasks)
                    {
                        task.Password = _dataProtectionService.Unprotect(task.Password);
                        task.OtpSecret = _dataProtectionService.Unprotect(task.OtpSecret);
                        var idFromDevice = await ExecuteRemoteTask(remoteDevice, task);
                        await TaskCompleted(task.Id, idFromDevice);
                    }

                    tasks = await _deviceTaskRepository.Query()
                        .Include(t => t.DeviceAccount)
                        .Where(t => t.DeviceId == deviceId)
                        .OrderBy(x => x.CreatedAt)
                        .ToListAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{deviceId}] {ex.Message}");
            }
            return false;
        }

        private async Task TaskCompleted(string taskId, ushort idFromDevice)
        {
            // Task
            var deviceTask = await _deviceTaskRepository
                .Query()
                .Include(d => d.DeviceAccount)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (deviceTask == null)
            {
                throw new Exception($"DeviceTask {taskId} not found");
            }

            // Device
            var device = await _deviceRepository.GetByIdAsync(deviceTask.DeviceId);

            // Device Account
            var deviceAccount = deviceTask.DeviceAccount;

            var properties = new List<string>() { "Status", "LastSyncedAt" };

            // Set value depending on the operation
            switch (deviceTask.Operation)
            {
                case TaskOperation.Create:
                    deviceAccount.Status = AccountStatus.Done;
                    deviceAccount.LastSyncedAt = DateTime.UtcNow;
                    deviceAccount.IdFromDevice = idFromDevice;
                    properties.Add("IdFromDevice");
                    await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties.ToArray());
                    _logger.LogInformation($"[{device.Id}] Task operation {deviceTask.Operation.ToString()}");
                    break;
                case TaskOperation.Update:
                    deviceAccount.Status = AccountStatus.Done;
                    deviceAccount.LastSyncedAt = DateTime.UtcNow;
                    if (deviceTask.Name != null)
                    {
                        deviceAccount.Name = deviceTask.Name;
                        properties.Add("Name");
                    }
                    if (deviceTask.Urls != null)
                    {
                        deviceAccount.Urls = deviceTask.Urls;
                        properties.Add("Urls");
                    }
                    if (deviceTask.Apps != null)
                    {
                        deviceAccount.Apps = deviceTask.Apps;
                        properties.Add("Apps");
                    }
                    if (deviceTask.Login != null)
                    {
                        deviceAccount.Login = deviceTask.Login;
                        properties.Add("Login");
                    }
                    if (deviceTask.Password != null)
                    {
                        deviceAccount.PasswordUpdatedAt = DateTime.UtcNow;
                        properties.Add("PasswordUpdatedAt");
                    }
                    if (deviceTask.OtpSecret != null)
                    {
                        deviceAccount.OtpUpdatedAt = deviceTask.OtpSecret != string.Empty ? new DateTime?(DateTime.UtcNow) : null;
                        properties.Add("OtpUpdatedAt");
                    }
                    await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties.ToArray());
                    _logger.LogInformation($"[{device.Id}] Task operation {deviceTask.Operation.ToString()}");
                    break;
                case TaskOperation.Delete:
                    deviceAccount.Status = AccountStatus.Done;
                    deviceAccount.LastSyncedAt = DateTime.UtcNow;
                    if (device.PrimaryAccountId == deviceAccount.Id)
                    {
                        device.PrimaryAccountId = null;
                        await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "PrimaryAccountId" });
                    }
                    deviceAccount.Deleted = true;
                    properties.Add("Deleted");
                    await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties.ToArray());
                    _logger.LogInformation($"[{device.Id}] Task operation {deviceTask.Operation.ToString()}");
                    break;
                case TaskOperation.Primary:
                    deviceAccount.Status = AccountStatus.Done;
                    deviceAccount.LastSyncedAt = DateTime.UtcNow;
                    device.PrimaryAccountId = deviceTask.DeviceAccountId;
                    await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "PrimaryAccountId" });
                    await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties.ToArray());
                    _logger.LogInformation($"[{device.Id}] Task operation {deviceTask.Operation.ToString()}");
                    break;
                case TaskOperation.Wipe:
                    device.MasterPassword = null;
                    await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "MasterPassword" });
                    _logger.LogInformation($"[{device.Id}] Task operation {deviceTask.Operation.ToString()}");
                    break;
                case TaskOperation.UnlockPin:
                    device.State = DeviceState.OK;
                    await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "State" });
                    _logger.LogInformation($"[{device.Id}] Task operation {deviceTask.Operation.ToString()}");
                    break;
                case TaskOperation.Link:
                    _logger.LogInformation($"[{device.Id}] Task operation {deviceTask.Operation.ToString()}");
                    break;
                case TaskOperation.Profile:
                    _logger.LogInformation($"[{device.Id}] Task operation {deviceTask.Operation.ToString()}");
                    break;
                default:
                    _logger.LogWarning($"[{device.Id}] unhandled case {deviceTask.Operation.ToString()}");
                    break;
            }

            // Delete task
            await _deviceTaskRepository.DeleteAsync(deviceTask);
            // Update UI use SognalR
            await Task.Delay(500);
            await _hubContext.Clients.All.SendAsync("ReloadPage", deviceAccount?.EmployeeId);
        }

        private async Task<ushort> ExecuteRemoteTask(RemoteDevice remoteDevice, DeviceTask task)
        {
            ushort idFromDevice = 0;
            switch (task.Operation)
            {
                case TaskOperation.Create:
                    idFromDevice = await AddDeviceAccount(remoteDevice, task);
                    break;
                case TaskOperation.Update:
                    idFromDevice = await UpdateDeviceAccount(remoteDevice, task);
                    break;
                case TaskOperation.Delete:
                    idFromDevice = await DeleteDeviceAccount(remoteDevice, task);
                    break;
                case TaskOperation.Wipe:
                    idFromDevice = await WipeDevice(remoteDevice, task);
                    break;
                case TaskOperation.Link:
                    idFromDevice = await LinkDevice(remoteDevice, task);
                    break;
                case TaskOperation.Primary:
                    idFromDevice = await SetDeviceAccountAsPrimary(remoteDevice, task);
                    break;
                case TaskOperation.Profile:
                    idFromDevice = await ProfileDevice(remoteDevice, task);
                    break;
                case TaskOperation.UnlockPin:
                    idFromDevice = await UnlockPin(remoteDevice, task);
                    break;
            }
            return idFromDevice;
        }

        private async Task<ushort> AddDeviceAccount(RemoteDevice remoteDevice, DeviceTask task)
        {
            var dev = await _deviceRepository
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.DeviceId);

            bool isPrimary = dev.PrimaryAccountId == task.DeviceAccountId;

            var pm = new DevicePasswordManager(remoteDevice, null);

            ushort key = task.DeviceAccount.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, task.Name, task.Password, task.Login, task.OtpSecret, task.Apps, task.Urls, isPrimary);

            return key;
        }

        private async Task<ushort> UpdateDeviceAccount(RemoteDevice remoteDevice, DeviceTask task)
        {
            var dev = await _deviceRepository
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.DeviceId);

            bool isPrimary = dev.PrimaryAccountId == task.DeviceAccountId;

            var pm = new DevicePasswordManager(remoteDevice, null);

            ushort key = task.DeviceAccount.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, task.Name, task.Password, task.Login, task.OtpSecret, task.Apps, task.Urls, isPrimary);

            return key;
        }

        private async Task<ushort> SetDeviceAccountAsPrimary(RemoteDevice remoteDevice, DeviceTask task)
        {
            var pm = new DevicePasswordManager(remoteDevice, null);

            ushort key = task.DeviceAccount.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, null, null, null, null, null, null, true);

            return key;
        }

        private async Task<ushort> DeleteDeviceAccount(RemoteDevice remoteDevice, DeviceTask task)
        {
            var device = await _deviceRepository
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.DeviceId);

            bool isPrimary = device.PrimaryAccountId == task.DeviceAccountId;
            var pm = new DevicePasswordManager(remoteDevice, null);
            ushort key = task.DeviceAccount.IdFromDevice;
            await pm.DeleteAccount(key, isPrimary);
            return 0;
        }

        private async Task<ushort> WipeDevice(RemoteDevice remoteDevice, DeviceTask task)
        {
            if (remoteDevice.AccessLevel.IsLinkRequired == true)
                return 0; // wipe is not required in this case

            var key = ConvertUtils.HexStringToBytes(task.Password);
            var respData = await remoteDevice.Wipe(key);
            return 0;
        }

        private async Task<ushort> LinkDevice(RemoteDevice remoteDevice, DeviceTask task)
        {
            var device = await _deviceRepository
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.DeviceId);

            if (device == null)
                throw new Exception($"Device not found");

            if (device.DeviceAccessProfile == null)
                throw new Exception($"DeviceAccessProfile is not set for {device.Id}");

            // Set Link   
            var key = ConvertUtils.HexStringToBytes(task.Password);
            if (remoteDevice.AccessLevel.IsLinkRequired)
            {
                await remoteDevice.Link(key);
            }

            // Set default profile
            var accessParams = new AccessParams()
            {
                MasterKey_Bond = device.DeviceAccessProfile.MasterKeyBonding,
                MasterKey_Connect = device.DeviceAccessProfile.MasterKeyConnection,
                MasterKey_Link = device.DeviceAccessProfile.MasterKeyNewLink,
                MasterKey_Channel = device.DeviceAccessProfile.MasterKeyNewChannel,

                Button_Bond = device.DeviceAccessProfile.ButtonBonding,
                Button_Connect = device.DeviceAccessProfile.ButtonConnection,
                Button_Link = device.DeviceAccessProfile.ButtonNewLink,
                Button_Channel = device.DeviceAccessProfile.ButtonNewChannel,

                Pin_Bond = device.DeviceAccessProfile.PinBonding,
                Pin_Connect = device.DeviceAccessProfile.ButtonConnection,
                Pin_Link = device.DeviceAccessProfile.PinNewLink,
                Pin_Channel = device.DeviceAccessProfile.PinNewChannel,

                PinMinLength = device.DeviceAccessProfile.PinLength,
                PinMaxTries = device.DeviceAccessProfile.PinTryCount,
                MasterKeyExpirationPeriod = device.DeviceAccessProfile.MasterKeyExpiration,
                PinExpirationPeriod = device.DeviceAccessProfile.PinExpiration,
                ButtonExpirationPeriod = device.DeviceAccessProfile.ButtonExpiration,
            };

            await remoteDevice.Access(DateTime.UtcNow, key, accessParams);

            return 0;
        }

        private async Task<ushort> ProfileDevice(RemoteDevice remoteDevice, DeviceTask task)
        {
            var device = await _deviceRepository
                .Query()
                .Include(d => d.DeviceAccessProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == task.DeviceId);

            var accessParams = new AccessParams()
            {
                MasterKey_Bond = device.DeviceAccessProfile.MasterKeyBonding,
                MasterKey_Connect = device.DeviceAccessProfile.MasterKeyConnection,
                MasterKey_Link = device.DeviceAccessProfile.MasterKeyNewLink,
                MasterKey_Channel = device.DeviceAccessProfile.MasterKeyNewChannel,

                Button_Bond = device.DeviceAccessProfile.ButtonBonding,
                Button_Connect = device.DeviceAccessProfile.ButtonConnection,
                Button_Link = device.DeviceAccessProfile.ButtonNewLink,
                Button_Channel = device.DeviceAccessProfile.ButtonNewChannel,

                Pin_Bond = device.DeviceAccessProfile.PinBonding,
                Pin_Connect = device.DeviceAccessProfile.ButtonConnection,
                Pin_Link = device.DeviceAccessProfile.PinNewLink,
                Pin_Channel = device.DeviceAccessProfile.PinNewChannel,

                PinMinLength = device.DeviceAccessProfile.PinLength,
                PinMaxTries = device.DeviceAccessProfile.PinTryCount,
                MasterKeyExpirationPeriod = device.DeviceAccessProfile.MasterKeyExpiration,
                PinExpirationPeriod = device.DeviceAccessProfile.PinExpiration,
                ButtonExpirationPeriod = device.DeviceAccessProfile.ButtonExpiration,
            };

            var key = ConvertUtils.HexStringToBytes(task.Password);

            await remoteDevice.Access(DateTime.UtcNow, key, accessParams);

            return 0;
        }

        private async Task<ushort> UnlockPin(RemoteDevice remoteDevice, DeviceTask task)
        {
            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Unlock(key);

            return 0;
        }
    }
}