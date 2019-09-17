using HES.Core.Interfaces;
using HES.Core.Services;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

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
        private readonly IDeviceService _deviceService;
        private readonly ILogger<DeviceHub> _logger;

        public DeviceHub(IRemoteTaskService remoteTaskService, 
                         IDeviceService deviceService,
                         ILogger<DeviceHub> logger)
        {
            _remoteTaskService = remoteTaskService;
            _deviceService = deviceService;
            _logger = logger;
        }

        // HUB connection is connected
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            string deviceId = httpContext.Request.Headers["DeviceId"].ToString();
            string channel = httpContext.Request.Headers["DeviceChannel"].ToString();
            byte channelNo = Convert.ToByte(channel);
            Debug.WriteLine($"!!!!!!!!!!!!!!!!!!!!! OnConnectedAsync {deviceId}:{channelNo}");

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                _logger.LogCritical($"DeviceId cannot be empty");
                await base.OnConnectedAsync();
            }

            var remoteDevice = new RemoteDevice(deviceId, Clients.Caller, null, new SdkLogger(_logger));

            Context.Items.Add("DeviceId", deviceId);
            Context.Items.Add("Device", remoteDevice);

            var access = await GetDeviceAccessParams(deviceId);

            var t = Task.Run(async () => 
            {
                try
                {
                    // initialize RemoteDevice before adding to the _connections
                    await remoteDevice.Verify(channelNo);
                    await remoteDevice.Initialize();

                    if (remoteDevice.AccessLevel.IsMasterKeyRequired)
                        await remoteDevice.Access(DateTime.UtcNow, access.Item2, access.Item1);

                    if (!_connections.TryAdd(deviceId, remoteDevice))
                        throw new Exception($"RemoteDevice already in the list of the connected devices");

                    if (_pendingConnections.TryGetValue(deviceId, out PendingConnectionDescription pendingConnection))
                    {
                        // inform clients about connection ready
                        pendingConnection.Tcs.TrySetResult(remoteDevice);
                        _pendingConnections.TryRemove(deviceId, out PendingConnectionDescription _);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex.Message);

                    if (_pendingConnections.TryGetValue(deviceId, out PendingConnectionDescription pendingConnection))
                    {
                        // inform clients about connection fail
                        pendingConnection.Tcs.TrySetException(ex);
                        _pendingConnections.TryRemove(deviceId, out PendingConnectionDescription _);
                    }

                    //todo - disconnect client
                    //Clients.Caller.
                }
            });
            

            await base.OnConnectedAsync();
        }

        async Task<Tuple<AccessParams, byte[]>> GetDeviceAccessParams(string deviceId)
        {
            var device = await _deviceService
                    .Query()
                    .Include(d => d.DeviceAccessProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device == null)
                throw new HideezException(HideezErrorCode.HesDeviceNotFound);

            if (device.DeviceAccessProfile == null)
                throw new HideezException(HideezErrorCode.HesEmptyDeviceAccessProfile);

            if (string.IsNullOrWhiteSpace(device.MasterPassword))
                throw new HideezException(HideezErrorCode.HesEmptyMasterKey);

            var key = ConvertUtils.HexStringToBytes(device.MasterPassword);

            var accessParams = new AccessParams()
            {
                MasterKey_Bond = device.DeviceAccessProfile.MasterKeyBonding,
                MasterKey_Connect = device.DeviceAccessProfile.MasterKeyConnection,
                MasterKey_Link = device.DeviceAccessProfile.MasterKeyNewLink,
                MasterKey_Channel = device.DeviceAccessProfile.MasterKeyNewChannel,

                Button_Bond = device.DeviceAccessProfile.ButtonBonding,
                Button_Connect = device.DeviceAccessProfile.ButtonConnection,
                Button_Link = device.DeviceAccessProfile.ButtonNewLink,
                Button_Channel = device.DeviceAccessProfile.ButtonNewChannel,

                Pin_Bond = device.DeviceAccessProfile.PinBonding,
                Pin_Connect = device.DeviceAccessProfile.ButtonConnection,
                Pin_Link = device.DeviceAccessProfile.PinNewLink,
                Pin_Channel = device.DeviceAccessProfile.PinNewChannel,

                PinMinLength = device.DeviceAccessProfile.PinLength,
                PinMaxTries = device.DeviceAccessProfile.PinTryCount,
                MasterKeyExpirationPeriod = device.DeviceAccessProfile.MasterKeyExpiration,
                PinExpirationPeriod = device.DeviceAccessProfile.PinExpiration,
                ButtonExpirationPeriod = device.DeviceAccessProfile.ButtonExpiration,
            };

            return new Tuple<AccessParams, byte[]>(accessParams, key);
        }

        // HUB connection is disconnected (OnDeviceDisconnected received in AppHub)
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

        internal static RemoteDevice FindDevice(string id)
        {
            if (_connections.TryGetValue(id, out RemoteDevice device))
                return device;

            return null;
        }

        internal static bool RemoveDevice(string id)
        {
            _pendingConnections.TryRemove(id, out PendingConnectionDescription _);
            return _connections.TryRemove(id, out RemoteDevice _);
        }

        // gets a device from the context. The device may not be present in the _connections at the moment
        // required for OnVerifyResponse and OnCommandResponse
        RemoteDevice GetDevice()
        {
            if (Context.Items.TryGetValue("Device", out object device))
                return (RemoteDevice)device;
            throw new Exception($"Cannot find device in the DeviceHub");
        }

        internal static async Task<RemoteDevice> WaitDeviceConnection(string id, byte channelNo, int timeout)
        {
            var descr = _pendingConnections.AddOrUpdate(id, new PendingConnectionDescription(id), (deviceId, oldDescr) =>
            {
                return oldDescr;
            });

            try
            {
                return await descr.Tcs.Task.TimeoutAfter(timeout);
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