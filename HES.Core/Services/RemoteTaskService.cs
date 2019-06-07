using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HES.Core.Services
{
    public class RemoteTaskService : IRemoteTaskService
    {
        readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;
        readonly IAsyncRepository<Device> _deviceRepository;
        private readonly ILogger<RemoteTaskService> _logger;
        private readonly IDataProtectionService _dataProtectionService;

        public RemoteTaskService(IAsyncRepository<DeviceAccount> deviceAccountRepository,
                                 IAsyncRepository<DeviceTask> deviceTaskRepository,
                                 IAsyncRepository<Device> deviceRepository,
                                 ILogger<RemoteTaskService> logger,
                                 IDataProtectionService dataProtectionService)
        {
            _deviceAccountRepository = deviceAccountRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _deviceRepository = deviceRepository;
            _logger = logger;
            _dataProtectionService = dataProtectionService;
        }

        public async Task AddTaskAsync(DeviceTask deviceTask)
        {
            await _deviceTaskRepository.AddAsync(deviceTask);
            ExecuteTask(deviceTask);
        }

        public async Task AddRangeTaskAsync(IList<DeviceTask> deviceTasks)
        {
            await _deviceTaskRepository.AddRangeAsync(deviceTasks);
            ExecuteTask(deviceTasks);
        }

        public async Task UndoLastTaskAsync(string accountId)
        {
            // Get account
            var deviceAccount = await _deviceAccountRepository.GetByIdAsync(accountId);
            // Get undo task
            var undoTask = await _deviceTaskRepository.Query().OrderByDescending(t => t.CreatedAt).FirstOrDefaultAsync(t => t.DeviceAccountId == accountId);
            // Delete Task
            await _deviceTaskRepository.DeleteAsync(undoTask);
            // Get last task
            var lastTask = _deviceTaskRepository.Query().Where(d => d.DeviceAccountId == deviceAccount.Id).OrderByDescending(d => d.CreatedAt).FirstOrDefault();
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
            // Get all tasks for this device
            var allTasks = _deviceTaskRepository.Query().Where(d => d.DeviceAccount.DeviceId == device.Id).ToList();
            // Delete all tasks
            await _deviceTaskRepository.DeleteRangeAsync(allTasks);

            // Get all accounts for this device
            var allAccounts = _deviceAccountRepository.Query().Where(d => d.DeviceId == device.Id).ToList();
            // Delete all acc
            await _deviceAccountRepository.DeleteRangeAsync(allAccounts);

            await AddTaskAsync(new DeviceTask { Password = _dataProtectionService.Protect(device.MasterPassword), CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Wipe, DeviceId = device.Id });
        }
        
        private void ExecuteTask(DeviceTask deviceTask)
        {
            Task.Run(async () =>
            {
                try
                {
                    var device = await _deviceRepository.GetByIdAsync(deviceTask.DeviceId);
                    await ExecuteRemoteTasks(device.MAC);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });
        }

        private void ExecuteTask(IList<DeviceTask> deviceTasks)
        {
            var devices = deviceTasks.Select(d => d.DeviceId).Distinct();
            foreach (var deviceId in devices)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var device = await _deviceRepository.GetByIdAsync(deviceId);
                        await ExecuteRemoteTasks(device.MAC);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                });
            }
        }
        
        public async Task ExecuteRemoteTasks(string deviceMac)
        {
            try
            {
                if (!_dataProtectionService.CanUse())
                {
                    throw new Exception("Data protection not activated or is busy.");
                }

                var currentDevice = await _deviceRepository.Query().Where(d => d.MAC == deviceMac).FirstOrDefaultAsync();
                var tasks = await _deviceTaskRepository.Query().Include(t => t.DeviceAccount).Where(t => t.DeviceId == currentDevice.Id).ToListAsync();

                if (tasks.Any())
                {
                    var remoteDevice = await AppHub.EstablishRemoteConnection(deviceMac, 4);

                    if (remoteDevice != null)
                    {
                        foreach (var task in tasks.OrderBy(x => x.CreatedAt))
                        {
                            task.Password = task.Password != null ? _dataProtectionService.Unprotect(task.Password) : null;
                            task.OtpSecret = task.OtpSecret != null ? _dataProtectionService.Unprotect(task.OtpSecret) : null;
                            var idFromDevice = await ExecuteRemoteTask(remoteDevice, task);
                            await TaskCompleted(task.Id, idFromDevice);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task TaskCompleted(string taskId, ushort idFromDevice)
        {
            // Get task
            var deviceTask = await _deviceTaskRepository.Query().Include(d => d.DeviceAccount).FirstOrDefaultAsync(t => t.Id == taskId);
            // Account for update
            var deviceAccount = deviceTask.DeviceAccount;

            if (deviceAccount != null)
            {
                List<string> properties = new List<string>() { "Status", "LastSyncedAt" };
                deviceAccount.Status = AccountStatus.Done;
                deviceAccount.LastSyncedAt = DateTime.UtcNow;
                deviceAccount.IdFromDevice = idFromDevice;

                // Set value depending on the operation
                switch (deviceTask.Operation)
                {
                    case TaskOperation.Create:
                        properties.Add("IdFromDevice");
                        await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties.ToArray());
                        break;
                    case TaskOperation.Update:
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
                        break;
                    case TaskOperation.Delete:
                        deviceAccount.Deleted = true;
                        properties.Add("Deleted");
                        await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties.ToArray());
                        break;
                    case TaskOperation.Primary:
                        await _deviceRepository.UpdateOnlyPropAsync(new Device { Id = deviceTask.DeviceId, PrimaryAccountId = deviceTask.DeviceAccountId }, new string[] { "PrimaryAccountId" });
                        await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties.ToArray());
                        break;
                    case TaskOperation.Wipe:
                        var device = await _deviceRepository.GetByIdAsync(deviceTask.DeviceId);
                        device.MasterPassword = null;
                        await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "MasterPassword" });
                        break;
                }
            }
            // Delete task
            await _deviceTaskRepository.DeleteAsync(deviceTask);
        }

        private async Task<ushort> ExecuteRemoteTask(RemoteDevice device, DeviceTask task)
        {
            ushort idFromDevice = 0;
            switch (task.Operation)
            {
                case TaskOperation.Create:
                    idFromDevice = await AddDeviceAccount(device, task);
                    break;
                case TaskOperation.Update:
                    idFromDevice = await UpdateDeviceAccount(device, task);
                    break;
                case TaskOperation.Delete:
                    idFromDevice = await DeleteDeviceAccount(device, task);
                    break;
                case TaskOperation.Wipe:
                    idFromDevice = await WipeDevice(device, task);
                    break;
                case TaskOperation.Link:
                    idFromDevice = await LinkDevice(device, task);
                    break;
            }
            return idFromDevice;
        }

        private async Task<ushort> AddDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var dev = await _deviceRepository.Query().Where(d => d.Id == task.DeviceId).AsNoTracking().FirstOrDefaultAsync();
            bool isPrimary = (dev.PrimaryAccountId == task.DeviceAccountId);

            var pm = new DevicePasswordManager(device);

            ushort key = task.DeviceAccount.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, 0x0000, task.Name, task.Password, task.Login, task.OtpSecret, task.Apps, task.Urls, isPrimary);

            return key;
        }

        private async Task<ushort> UpdateDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var dev = await _deviceRepository.Query().Where(d => d.Id == task.DeviceId).AsNoTracking().FirstOrDefaultAsync();
            bool isPrimary = (dev.PrimaryAccountId == task.DeviceAccountId);

            var pm = new DevicePasswordManager(device);

            ushort key = task.DeviceAccount.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, 0x0000, task.Name, task.Password, task.Login, task.OtpSecret, task.Apps, task.Urls, isPrimary);

            return key;
        }

        private async Task<ushort> DeleteDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var dev = await _deviceRepository.Query().Where(d => d.Id == task.DeviceId).AsNoTracking().FirstOrDefaultAsync();
            bool isPrimary = (dev.PrimaryAccountId == task.DeviceAccountId);
            var pm = new DevicePasswordManager(device);
            ushort key = (ushort)task.DeviceAccount.IdFromDevice;
            await pm.DeleteAccount(key, isPrimary);
            return 0;
        }

        private async Task<ushort> WipeDevice(RemoteDevice device, DeviceTask task)
        {
            var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var respData = await device.Ping(pingData);
            Debug.Assert(pingData.SequenceEqual(respData.Result));
            return 0;
        }

        private async Task<ushort> LinkDevice(RemoteDevice device, DeviceTask task)
        {
            var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var respData = await device.Ping(pingData);
            Debug.Assert(pingData.SequenceEqual(respData.Result));
            return 0;
        }
    }
}