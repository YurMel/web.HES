using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceTaskService
    {
        IQueryable<DeviceTask> Query();
        Task<int> GetCountAsync();
        Task AddTaskAsync(DeviceTask deviceTask);
        Task AddRangeAsync(IList<DeviceTask> deviceTasks);
        Task UndoLastTaskAsync(string accountId);
        Task RemoveAllTasksAsync(string deviceId);
    }
}