using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.Remote;

namespace HES.Core.Services
{
    public class RemoteTaskService : IRemoteTaskService
    {
        readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;

        public RemoteTaskService(IAsyncRepository<DeviceAccount> deviceAccountRepository,
                                 IAsyncRepository<DeviceTask> deviceTaskRepository)
        {
            _deviceAccountRepository = deviceAccountRepository;
            _deviceTaskRepository = deviceTaskRepository;
        }

        public async Task AddTaskAsync(DeviceTask deviceTask)
        {
            await _deviceTaskRepository.AddAsync(deviceTask);
        }

        public async Task AddRangeTaskAsync(IList<DeviceTask> deviceTasks)
        {
            await _deviceTaskRepository.AddRangeAsync(deviceTasks);
        }

        public async Task UndoLastTaskAsync(string taskId)
        {
            // Get Task and Acc
            var deviceTask = await _deviceTaskRepository.GetByIdAsync(taskId);
            var deviceAccount = await _deviceAccountRepository.GetByIdAsync(deviceTask.DeviceAccountId);
            // Delete Task
            await _deviceTaskRepository.DeleteAsync(deviceTask);
            // Get last task
            var lastTask = _deviceTaskRepository.Query().Where(d => d.DeviceAccountId == deviceAccount.Id).OrderByDescending(d => d.CreatedAt).FirstOrDefault();
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
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, new string[] { "Status", "UpdatedAt" });
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

            await AddTaskAsync(new DeviceTask { CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Wipe });
        }

        private async Task TaskCompleted(string taskId, short idFromDevice)
        {
            var deviceTask = await _deviceTaskRepository.GetByIdAsync(taskId);
            var deviceAccount = await _deviceAccountRepository.GetByIdAsync(deviceTask.DeviceAccountId);

            List<string> properties = new List<string>() { "Status", "LastSyncedAt" };
            deviceAccount.Status = AccountStatus.Done;
            deviceAccount.LastSyncedAt = DateTime.UtcNow;
            deviceAccount.IdFromDevice = idFromDevice;

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
                case TaskOperation.Wipe:
                    break;
            }
            await _deviceTaskRepository.DeleteAsync(deviceTask);
        }

        public async Task ExecuteRemoteTasks(string deviceMac)
        {
            var tasks = _deviceTaskRepository.Query()
                .Where(t => t.DeviceAccount.Device.MAC == deviceMac);

            if (tasks.Any())
            {
                var remoteDevice = await AppHub.EstablishRemoteConnection(deviceMac, 4);

                if (remoteDevice != null)
                {
                    foreach (var task in tasks)
                    {
                        var idFromDevice = await ExecuteRemoteTask(remoteDevice, task);
                        await TaskCompleted(task.Id, idFromDevice);
                    }
                }
            }
        }

        private async Task<short> ExecuteRemoteTask(RemoteDevice device, DeviceTask task)
        {
            short idFromDevice = 0;
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
            }
            return idFromDevice;
        }

        private async Task<short> AddDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var respData = await device.Ping(pingData);
            Debug.Assert(pingData.SequenceEqual(respData.Result));
            return 0;
        }

        private async Task<short> UpdateDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var respData = await device.Ping(pingData);
            Debug.Assert(pingData.SequenceEqual(respData.Result));
            return 0;
        }

        private async Task<short> DeleteDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var respData = await device.Ping(pingData);
            Debug.Assert(pingData.SequenceEqual(respData.Result));
            return 0;
        }

        private async Task<short> WipeDevice(RemoteDevice device, DeviceTask task)
        {
            var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var respData = await device.Ping(pingData);
            Debug.Assert(pingData.SequenceEqual(respData.Result));
            return 0;
        }
    }
}