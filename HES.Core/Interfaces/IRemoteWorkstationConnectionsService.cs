using System.Collections.Generic;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Workstation;

namespace HES.Core.Interfaces
{
    public interface IRemoteWorkstationConnectionsService
    {
        void StartUpdateRemoteDevice(IList<string> devicesId);
        void StartUpdateRemoteDevice(string deviceId);
        Task<HideezErrorInfo> UpdateRemoteDeviceAsync(string deviceId, string workstationId);
        Task<HideezErrorInfo> RegisterWorkstationInfo(IRemoteAppConnection remoteAppConnection, WorkstationInfo workstationInfo);
        Task OnWorkstationDisconnected(string workstationId);
    }
}
