using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HES.Core.Hubs
{
    public class DeviceHub : Hub<IRemoteCommands>
    {
        class PendingConnectionDescription
        {
            public string DeviceId { get; }
            public TaskCompletionSource<RemoteDevice> Tcs { get; } = new TaskCompletionSource<RemoteDevice>();

            public PendingConnectionDescription(string deviceId)
            {
                DeviceId = deviceId;
            }
        }

        static readonly ConcurrentDictionary<string, PendingConnectionDescription> _pendingConnections
            = new ConcurrentDictionary<string, PendingConnectionDescription>();

        static readonly ConcurrentDictionary<string, RemoteDevice> _connections
            = new ConcurrentDictionary<string, RemoteDevice>();

        private readonly IRemoteTaskService _remoteTaskService;
        private readonly ILogger<DeviceHub> _logger;
        
        public DeviceHub(IRemoteTaskService remoteTaskService, ILogger<DeviceHub> logger)
        {
            _remoteTaskService = remoteTaskService;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            string deviceId = httpContext.Request.Headers["DeviceId"].ToString();
            string channel = httpContext.Request.Headers["DeviceChannel"].ToString();
            byte channelNo = Convert.ToByte(channel);
            Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!! OnConnectedAsync {deviceId}");


            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                var device = new RemoteDevice(deviceId, Clients.Caller, null);

                Context.Items.Add("DeviceId", deviceId);
                Context.Items.Add("Device", device);

                if (_connections.TryAdd(deviceId, device))
                {
                    var t = Task.Run(async () =>
                    {
                        try
                        {
                            await device.Verify(channelNo);
                            if (_pendingConnections.TryGetValue(deviceId, out PendingConnectionDescription pendingConnection))
                            {
                                pendingConnection.Tcs.TrySetResult(device);
                                _pendingConnections.TryRemove(deviceId, out PendingConnectionDescription removedPendingConnection);
                            }

                            _remoteTaskService.StartTaskProcessing(deviceId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex.Message);
                        }
                    });
                }
            }

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!! OnDisconnectedAsync");

            if (Context.Items.TryGetValue("DeviceId", out object deviceId))
            {
                RemoveDevice((string)deviceId);
            }
            else
            {
                Debug.Assert(false);
                _logger.LogCritical("DeviceHub does not contain DeviceId!");
            }

            return base.OnDisconnectedAsync(exception);
        }

        public static RemoteDevice FindDevice(string id)
        {
            if (_connections.TryGetValue(id, out RemoteDevice device))
                return device;

            return null;
        }

        public static bool RemoveDevice(string id)
        {
            _pendingConnections.TryRemove(id, out PendingConnectionDescription removedPendingConnection);
            return _connections.TryRemove(id, out RemoteDevice device);
        }

        private RemoteDevice GetDevice()
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

        public Task OnVerifyResponse(byte[] data)
        {
            try
            {
                RemoteDevice device = GetDevice();
                device.OnVerifyResponse(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new HubException(ex.Message);
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Debug.WriteLine(ex.Message);
                throw new HubException(ex.Message);
            }
            return Task.CompletedTask;
        }
    }
}