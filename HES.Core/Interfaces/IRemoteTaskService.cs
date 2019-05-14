using HES.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IRemoteTaskService
    {
        Task AddTaskAsync(DeviceTask deviceTask);
        Task AddRangeTaskAsync(IList<DeviceTask> deviceTasks);
        Task UndoLastTaskAsync(string accountId);
        Task RemoveDeviceAsync(Device device);
        Task ExecuteRemoteTasks(string deviceMac);
    }
}