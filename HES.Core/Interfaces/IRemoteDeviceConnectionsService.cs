using Hideez.SDK.Communication;

namespace HES.Core.Interfaces
{
    public interface IRemoteDeviceConnectionsService
    {
        void MakeConnection(string deviceId, IRemoteCommands caller);
    }
}
