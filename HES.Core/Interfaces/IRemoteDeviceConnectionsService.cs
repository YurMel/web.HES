using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;

namespace HES.Core.Interfaces
{
    public interface IRemoteDeviceConnectionsService
    {
        void OnDeviceHubConnected(string deviceId, IRemoteCommands caller);
        void AddDevice(string deviceId, IRemoteAppConnection appConnection);
        void RemoveDevice(IRemoteAppConnection appConnection);
    }
}
