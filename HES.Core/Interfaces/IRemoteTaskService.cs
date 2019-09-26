using HES.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IRemoteTaskService
    {
        Task AddTaskAsync(DeviceTask deviceTask);
        Task AddRangeAsync(IList<DeviceTask> deviceTasks);

        Task UndoLastTaskAsync(string accountId);
        Task RemoveDeviceAsync(Device device);

        Task ProcessTasksAsync(string deviceId, TaskOperation operation);
        void StartTaskProcessing(string deviceId);
        void StartTaskProcessing(IList<string> deviceIdList);
    }
}