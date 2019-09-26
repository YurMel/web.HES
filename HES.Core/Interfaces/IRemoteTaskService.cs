using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IRemoteTaskService
    {
        Task ProcessTasksAsync(string deviceId);
        void StartTaskProcessing(string deviceId);
        void StartTaskProcessing(IList<string> deviceIdList);
    }
}