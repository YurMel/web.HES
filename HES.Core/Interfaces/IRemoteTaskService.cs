using System.Collections.Generic;
using System.Threading.Tasks;
using HES.Core.Entities;

namespace HES.Core.Interfaces
{
    public interface IRemoteTaskService
    {
        Task ProcessTasksAsync(string deviceId, TaskOperation operation);
        void StartTaskProcessing(string deviceId);
        void StartTaskProcessing(IList<string> deviceIdList);
    }
}