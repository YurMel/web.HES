using HES.Core.Entities;
using Hideez.SDK.Communication.Remote;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IRemoteTaskService
    {
        Task AddTaskAsync(DeviceTask deviceTask);
        Task AddRangeTaskAsync(IList<DeviceTask> deviceTask);
        Task UndoLastTaskAsync(string taskId);
        Task RemoveDeviceAsync(string employeeId, string deviceId);
        Task ExecuteRemoteTasks(string deviceMac);
    }
}