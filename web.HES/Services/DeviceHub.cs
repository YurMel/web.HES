using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using Microsoft.AspNetCore.SignalR;
using Hideez.SDK.Communication.Utils;

namespace web.HES.Services
{
    public class DeviceHub : Hub<IRemoteDeviceConnection>
    {
        class PendingConnectionDescription
        {
            public string Mac { get; }
            public TaskCompletionSource<RemoteDevice> Tcs { get; } = new TaskCompletionSource<RemoteDevice>();

            public PendingConnectionDescription(string mac)
            {
                Mac = mac;
            }
        }

        static readonly ConcurrentDictionary<string, PendingConnectionDescription> _pendingConnections
            = new ConcurrentDictionary<string, PendingConnectionDescription>();

        static readonly ConcurrentDictionary<string, RemoteDevice> _connections 
            = new ConcurrentDictionary<string, RemoteDevice>();

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            string mac = httpContext.Request.Headers["DeviceMac"].ToString();
            string channel = httpContext.Request.Headers["DeviceChannel"].ToString();
            byte channelNo = Convert.ToByte(channel);


            if (!string.IsNullOrWhiteSpace(mac))
            {
                var device = new RemoteDevice(mac, Clients.Caller);

                Context.Items.Add("DeviceMac", mac);
                Context.Items.Add("Device", device);

                if (_connections.TryAdd(mac, device))
                {
                    var t = Task.Run(async () =>
                    {
                        await device.Authenticate(channelNo);
                        if (_pendingConnections.TryGetValue(mac, out PendingConnectionDescription pendingConnection))
                        {
                            pendingConnection.Tcs.TrySetResult(device);
                            _pendingConnections.TryRemove(mac, out PendingConnectionDescription removedPendingConnection);
                        }
                    });
                }
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (Context.Items.TryGetValue("DeviceMac", out object deviceMac))
            {
                _connections.TryRemove((string)deviceMac, out RemoteDevice removedDevice);
                _pendingConnections.TryRemove((string)deviceMac, out PendingConnectionDescription removedPendingConnection);
            }
            else
            {
                Debug.Assert(false);
            }

            return base.OnDisconnectedAsync(exception);
        }

        public static RemoteDevice FindDevice(string id)
        {
            if (_connections.TryGetValue(id, out RemoteDevice device))
                return device;

            return null;
        }

        RemoteDevice GetDevice()
        {
            if (Context.Items.TryGetValue("Device", out object device))
                return (RemoteDevice)device;
            throw new Exception($"Cannot find device in the DeviceHub");
        }

        internal static async Task<RemoteDevice> WaitDeviceConnection(string id, int timeout)
        {
            var descr = _pendingConnections.AddOrUpdate(id, new PendingConnectionDescription(id), (deviceId, oldDescr) =>
            {
                return oldDescr;
            });

            try
            {
                var remoteDevice = await descr.Tcs.Task.TimeoutAfter(timeout);
                return remoteDevice;
            }
            catch (TimeoutException)
            {
                descr.Tcs.TrySetException(new HideezException(HideezErrorCode.RemoteConnectionTimedOut));
            }
            catch (Exception ex)
            {
                descr.Tcs.TrySetException(ex);
            }
            finally
            {
                _pendingConnections.TryRemove(id, out PendingConnectionDescription removed);
            }

            return null;
        }

        public Task OnAuthResponse(byte[] data)
        {
            try
            {
                RemoteDevice device = GetDevice();
                device.OnAuthResponse(data);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }

        public Task OnCommandResponse(byte[] data)
        {
            try
            {
                RemoteDevice device = GetDevice();
                device.OnCommandResponse(data);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }

    }
}
