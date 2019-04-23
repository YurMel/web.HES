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
        readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;

        public RemoteTaskService(IAsyncRepository<DeviceTask> deviceTaskRepository)
        {
            _deviceTaskRepository = deviceTaskRepository;
        }

        public async Task AddRangeTaskAsync(IList<DeviceTask> deviceTask)
        {
            await Task.CompletedTask;
        }

        public async Task AddTaskAsync(DeviceTask deviceTask)
        {
            await Task.CompletedTask;
        }

        public async Task RemoveDeviceAsync(string employeeId, string deviceId)
        {
            await Task.CompletedTask;
        }

        public async Task UndoLastTaskAsync(string taskId)
        {
            await Task.CompletedTask;
        }

        private async Task TaskCompleted(string taskId)
        {
            await Task.CompletedTask;
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
                        await ExecuteRemoteTask(remoteDevice, task);
                        await TaskCompleted(task.Id);
                    }
                }
            }
        }

        async Task ExecuteRemoteTask(RemoteDevice device, DeviceTask task)
        {
            switch (task.Operation)
            {
                case TaskOperation.Create:
                    await AddDeviceAccount(device, task);
                    break;
                case TaskOperation.Update:
                    await UpdateDeviceAccount(device, task);
                    break;
                case TaskOperation.Delete:
                    await DeleteDeviceAccount(device, task);
                    break;
                default:
                    break;
            }
        }

        async Task AddDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var respData = await device.Ping(pingData);
            Debug.Assert(pingData.SequenceEqual(respData.Result));
        }

        async Task DeleteDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var respData = await device.Ping(pingData);
            Debug.Assert(pingData.SequenceEqual(respData.Result));
        }

        async Task UpdateDeviceAccount(RemoteDevice device, DeviceTask task)
        {
            var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
            var respData = await device.Ping(pingData);
            Debug.Assert(pingData.SequenceEqual(respData.Result));
        }


    }
}