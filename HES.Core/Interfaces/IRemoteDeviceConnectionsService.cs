using Hideez.SDK.Communication;

namespace HES.Core.Interfaces
{
    public interface IRemoteDeviceConnectionsService
    {
        void OnDeviceHubConnected(string deviceId, IRemoteCommands caller);
    }
}
