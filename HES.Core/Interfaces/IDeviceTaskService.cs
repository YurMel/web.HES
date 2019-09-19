using HES.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceTaskService
    {
        Task AddTaskAsync(DeviceTask deviceTask);
        Task AddRangeAsync(IList<DeviceTask> deviceTasks);
        Task UndoLastTaskAsync(string accountId);
        Task RemoveAllTasksAsync(string deviceId);
    }
}