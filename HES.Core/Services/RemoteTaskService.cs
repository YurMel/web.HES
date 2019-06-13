using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Remote;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<EmployeeDetailsHub> _hubContext;

        public RemoteTaskService(IAsyncRepository<DeviceAccount> deviceAccountRepository,
                                 IAsyncRepository<DeviceTask> deviceTaskRepository,
                                 IAsyncRepository<Device> deviceRepository,
                                 ILogger<RemoteTaskService> logger,
                                 IDataProtectionService dataProtectionService,
                                 IHubContext<EmployeeDetailsHub> hubContext)
        {
            _deviceAccountRepository = deviceAccountRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _deviceRepository = deviceRepository;
            _logger = logger;
            _dataProtectionService = dataProtectionService;
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
            //// Get all tasks for this device
            //var allTasks = _deviceTaskRepository.Query()
            //    .Where(d => d.DeviceAccount.DeviceId == device.Id).ToList();

            //// Delete all tasks
            //await _deviceTaskRepository.DeleteRangeAsync(allTasks);

            //// Get all accounts for this device
            //var allAccounts = _deviceAccountRepository.Query()
            //    .Where(d => d.DeviceId == device.Id).ToList();

            //// Delete all accounts
            //await _deviceAccountRepository.DeleteRangeAsync(allAccounts);

            await AddTaskAsync(new DeviceTask
            {
                Password = _dataProtectionService.Protect(device.MasterPassword),
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Wipe,
                DeviceId = device.Id
            });
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
            Task.Run(async () =>
            {
                try
                {
                    await ExecuteRemoteTasks(deviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });
        }

        async Task ExecuteRemoteTasks(string deviceId)
        {
            try
            {
                _dataProtectionService.Validate();

                var tasks = await _deviceTaskRepository.Query()
                    .Include(t => t.DeviceAccount)
                    .Where(t => t.DeviceId == deviceId)
                    .ToListAsync();

                if (tasks.Any())
                {
                    var remoteDevice = await AppHub.EstablishRemoteConnection(deviceId, 4);

                    if (remoteDevice != null)
                    {
                        foreach (var task in tasks.OrderBy(x => x.CreatedAt))
                        {
                            task.Password = _dataProtectionService.Unprotect(task.Password);
                            task.OtpSecret = _dataProtectionService.Unprotect(task.OtpSecret);
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
            var deviceTask = await _deviceTaskRepository.Query()
                .Include(d => d.DeviceAccount)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (deviceTask == null)
                throw new Exception($"DeviceTask {taskId} not found");

            // Account for update
            var deviceAccount = deviceTask.DeviceAccount;

            if (deviceAccount != null)
            {
                var properties = new List<string>() { "Status", "LastSyncedAt" };
                deviceAccount.Status = AccountStatus.Done;
                deviceAccount.LastSyncedAt = DateTime.UtcNow;

                // Set value depending on the operation
                switch (deviceTask.Operation)
                {
                    case TaskOperation.Create:
                        deviceAccount.IdFromDevice = idFromDevice;
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
                        var device = await _deviceRepository.GetByIdAsync(deviceTask.DeviceId);
                        device.PrimaryAccountId = deviceTask.DeviceAccountId;
                        await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "PrimaryAccountId" });
                        await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties.ToArray());
                        break;
                    default:
                        _logger.LogCritical("Unknown deviceTask.Operation");
                        break;
                }
            }
            else
            {
                switch (deviceTask.Operation)
                {
                    case TaskOperation.Wipe:
                        var device = await _deviceRepository.GetByIdAsync(deviceTask.DeviceId);
                        device.MasterPassword = null;
                        await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "MasterPassword" });
                        break;
                    default:
                        _logger.LogCritical("Unknown deviceTask.Operation");
                        break;
                }
            }

            // Delete task
            await _deviceTaskRepository.DeleteAsync(deviceTask);
            // Update UI use SognalR
            await _hubContext.Clients.All.SendAsync("ReloadPage");
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
                case TaskOperation.Primary:
                    idFromDevice = await SetDeviceAccountAsPrimary(device, task);
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
            var dev = await _deviceRepository.Query()
                .Where(d => d.Id == task.DeviceId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            bool isPrimary = dev.PrimaryAccountId == task.DeviceAccountId;

            var pm = new DevicePasswordManager(device);

            ushort key = task.DeviceAccount.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, 0x0000, task.Name, task.Password, task.Login, task.OtpSecret, task.Apps, task.Urls, isPrimary);

            return key;
        }

        private async Task<ushort> UpdateDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var dev = await _deviceRepository.Query()
                .Where(d => d.Id == task.DeviceId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            bool isPrimary = dev.PrimaryAccountId == task.DeviceAccountId;

            var pm = new DevicePasswordManager(device);

            ushort key = task.DeviceAccount.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, 0x0000, task.Name, task.Password, task.Login, task.OtpSecret, task.Apps, task.Urls, isPrimary);

            return key;
        }

        private async Task<ushort> SetDeviceAccountAsPrimary(RemoteDevice device, DeviceTask task)
        {
            var pm = new DevicePasswordManager(device);

            ushort key = task.DeviceAccount.IdFromDevice;
            key = await pm.SaveOrUpdateAccount(key, 0x0000, null, null, null, null, null, null, true);

            return key;
        }

        private async Task<ushort> DeleteDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var dev = await _deviceRepository.Query()
                .Where(d => d.Id == task.DeviceId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            bool isPrimary = dev.PrimaryAccountId == task.DeviceAccountId;
            var pm = new DevicePasswordManager(device);
            ushort key = task.DeviceAccount.IdFromDevice;
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