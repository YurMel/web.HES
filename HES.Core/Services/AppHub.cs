using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using Microsoft.AspNetCore.SignalR;

namespace web.HES.Services
{
    public class AppHub : Hub<IRemoteAppConnection>
    {
        class DeviceDescription
        {
            public IRemoteAppConnection Connection { get; }

            public DeviceDescription(IRemoteAppConnection connection)
            {
                Connection = connection;
            }
        }

        static readonly ConcurrentDictionary<string, DeviceDescription> _deviceConnections
            = new ConcurrentDictionary<string, DeviceDescription>();

        public override Task OnConnectedAsync()
        {
            Context.Items.Add("DeviceMacs", new ConcurrentDictionary<string, string>());

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // remove all device connections for this app
            var deviceList = GetDeviceMacList();
            foreach (var item in deviceList.ToArray())
            {
                OnDeviceDisconnected(item.Key);
            }

            return base.OnDisconnectedAsync(exception);
        }

        ConcurrentDictionary<string, string> GetDeviceMacList()
        {
            if (Context.Items.TryGetValue("DeviceMacs", out object deviceMacs))
                return deviceMacs as ConcurrentDictionary<string, string>;

            Debug.Assert(false);

            var list = new ConcurrentDictionary<string, string>();
            Context.Items.Add("DeviceMacs", list);

            return list;
        }

        public Task OnDeviceConnected(string mac)
        {
            _deviceConnections.AddOrUpdate(mac, new DeviceDescription(Clients.Caller), (deviceMac, oldDescr) => 
            {
                return new DeviceDescription(Clients.Caller);
            });

            var deviceList = GetDeviceMacList();
            deviceList.TryAdd(mac, mac);

            return Task.CompletedTask;
        }

        public Task OnDeviceDisconnected(string mac)
        {
            _deviceConnections.TryRemove(mac, out DeviceDescription deviceDescription);

            var deviceList = GetDeviceMacList();
            deviceList.TryRemove(mac, out string removed);

            return Task.CompletedTask;
        }

        static DeviceDescription FindDeviceDescription(string id)
        {
            _deviceConnections.TryGetValue(id, out DeviceDescription device);
            return device;
        }

        internal static async Task<RemoteDevice> EstablishRemoteConnection(string id, byte channelNo)
        {
            try
            {
                var device = DeviceHub.FindDevice(id);
                if (device != null)
                    return device;

                var deviceDescr = FindDeviceDescription(id);
                if (deviceDescr == null)
                    throw new HideezException(HideezErrorCode.DeviceNotConnectedToAnyHost);

                await deviceDescr.Connection.EstablishRemoteDeviceConnection(id, channelNo);
                return await DeviceHub.WaitDeviceConnection(id, timeout: 3000);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw new HubException(ex.Message);
            }
        }

        internal static bool IsDeviceConnectedToHost(string id)
        {
            var device = FindDeviceDescription(id);
            return device != null;
        }
    }
}
