using System.Threading.Tasks;
using HES.Core.Entities;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;

namespace HES.Core.Interfaces
{
    public interface IRemoteTaskService
    {
        Task<HideezErrorCode> ExecuteRemoteTasks(string deviceId, RemoteDevice remoteDevice, TaskOperation operation);
    }
}