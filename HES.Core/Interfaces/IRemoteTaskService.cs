using System.Threading.Tasks;
using HES.Core.Entities;
using Hideez.SDK.Communication;

namespace HES.Core.Interfaces
{
    public interface IRemoteTaskService
    {
        Task<HideezErrorCode> ExecuteRemoteTasks(string deviceId, TaskOperation operation);
    }
}