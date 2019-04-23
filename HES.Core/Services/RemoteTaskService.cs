using HES.Core.Entities;
using HES.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteTaskService : IRemoteTaskService
    {
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
    }
}