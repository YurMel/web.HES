using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HES.Core.Interfaces;
using HES.Core.Services;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace HES.Core.Hubs
{
    public class DeviceHub : Hub<IRemoteCommands>
    {
        readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        readonly ILogger<DeviceHub> _logger;

        public DeviceHub(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                         ILogger<DeviceHub> logger)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _logger = logger;
        }

        // HUB connection is connected
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            string deviceId = httpContext.Request.Headers["DeviceId"].ToString();
            string channel = httpContext.Request.Headers["DeviceChannel"].ToString();
            Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!! OnConnectedAsync {deviceId}:{channel}");

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                _logger.LogCritical($"DeviceId cannot be empty");
            }
            else
            {
                _remoteDeviceConnectionsService.OnDeviceHubConnected(deviceId, Clients.Caller);
                Context.Items.Add("DeviceId", deviceId);
            }

            await base.OnConnectedAsync();
        }

        // HUB connection is disconnected (OnDeviceDisconnected received in AppHub)
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!! OnDisconnectedAsync");

            if (Context.Items.TryGetValue("DeviceId", out object deviceId))
            {
                RemoteDeviceConnectionsService.RemoveDevice((string)deviceId);
            }
            else
            {
                Debug.Assert(false);
                _logger.LogCritical("DeviceHub does not contain DeviceId!");
            }

            return base.OnDisconnectedAsync(exception);
        }

        // gets a device from the context
        RemoteDevice GetDevice()
        {
            RemoteDevice remoteDevice = null;

            if (Context.Items.TryGetValue("DeviceId", out object deviceId))
                remoteDevice = RemoteDeviceConnectionsService.FindDevice((string)deviceId);

            if (remoteDevice == null)
                throw new Exception($"Cannot find device in the DeviceHub");

            return remoteDevice;
        }

        // incoming request
        public Task OnVerifyResponse(byte[] data, string error)
        {
            try
            {
                var device = GetDevice();
                device.OnVerifyResponse(data, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                Debug.WriteLine(ex.Message);
                throw new HubException(ex.Message);
            }
            return Task.CompletedTask;
        }

        // incoming request
        public Task OnCommandResponse(byte[] data, string error)
        {
            try
            {
                var device = GetDevice();
                device.OnCommandResponse(data, error);
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