using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Microsoft.Extensions.Logging;

namespace HES.Core.Services
{
    public class RemoteDeviceConnectionsService : IRemoteDeviceConnectionsService
    {
        class DeviceDescription
        {
            public IRemoteAppConnection AppConnection { get; set; }
            public string DeviceId { get; set; }
            public RemoteDevice RemoteDevice { get; set; }

            public DeviceDescription(string deviceId, IRemoteAppConnection appConnection)
            {
                DeviceId = deviceId;
                AppConnection = appConnection;
            }
        }

        static readonly ConcurrentDictionary<string, TaskCompletionSource<RemoteDevice>> _pendingConnections
            = new ConcurrentDictionary<string, TaskCompletionSource<RemoteDevice>>();

        static readonly ConcurrentDictionary<string, DeviceDescription> _deviceDescriptions
            = new ConcurrentDictionary<string, DeviceDescription>();

        const byte channelNo = 4;

        readonly ILogger<RemoteDeviceConnectionsService> _logger;
        readonly IDataProtectionService _dataProtectionService;

        public RemoteDeviceConnectionsService(ILogger<RemoteDeviceConnectionsService> logger,
                                              IDataProtectionService dataProtectionService)
        {
            _logger = logger;
            _dataProtectionService = dataProtectionService;
        }

        public void AddDevice(string deviceId, IRemoteAppConnection appConnection)
        {
            var deviceDescription = _deviceDescriptions.GetOrAdd(deviceId, (x) =>
            {
                return new DeviceDescription(deviceId, appConnection);
            });

            deviceDescription.AppConnection = appConnection;
        }

        public void RemoveDevice(IRemoteAppConnection appConnection)
        {
            var devices = _deviceDescriptions.Values.Where(x => x.AppConnection == appConnection).ToList();
            foreach (var item in devices)
            {
                _deviceDescriptions.TryRemove(item.DeviceId, out DeviceDescription _);
            }
        }

        public static bool IsDeviceConnectedToHost(string deviceId)
        {
            return _deviceDescriptions.ContainsKey(deviceId);
        }

        public static async Task<RemoteDevice> Connect(string deviceId, byte channelNo)
        {
            _deviceDescriptions.TryGetValue(deviceId, out DeviceDescription deviceDescription);
            if (deviceDescription == null || deviceDescription.AppConnection == null)
                throw new HideezException(HideezErrorCode.DeviceNotConnectedToAnyHost);

            // return existing connection
            if (deviceDescription.RemoteDevice != null)
                return deviceDescription.RemoteDevice;

            var isNew = false;
            var tcs = _pendingConnections.GetOrAdd(deviceId, (x) =>
            {
                isNew = true;
                return new TaskCompletionSource<RemoteDevice>();
            });

            if (!isNew)
                return await tcs.Task;

            // call Hideez Client to make remote channel
            await deviceDescription.AppConnection.EstablishRemoteDeviceConnection(deviceId, channelNo);

            var remoteDevice = await WaitDeviceConnection(deviceId, timeout: 20_000);

            return remoteDevice;
        }

        public async void OnDeviceHubConnected(string deviceId, IRemoteCommands caller)
        {
            try
            {
                _deviceDescriptions.TryGetValue(deviceId, out DeviceDescription deviceDescription);
                if (deviceDescription == null)
                    throw new HideezException(HideezErrorCode.DeviceNotConnectedToAnyHost);

                deviceDescription.RemoteDevice = new RemoteDevice(deviceId, caller, null, new SdkLogger<RemoteDeviceConnectionsService>(_logger));

                await deviceDescription.RemoteDevice.Verify(channelNo);

                if (_pendingConnections.TryGetValue(deviceId, out TaskCompletionSource<RemoteDevice> tcs))
                {
                    // Inform clients about connection ready
                    tcs.TrySetResult(deviceDescription.RemoteDevice);
                    _pendingConnections.TryRemove(deviceId, out TaskCompletionSource<RemoteDevice> _);
                }


                //var remoteDevice = new RemoteDevice(deviceId, caller, null, new SdkLogger(_logger));

                //if (!_connections.TryAdd(deviceId, remoteDevice))
                //    throw new Exception($"RemoteDevice already in the list of the connected devices");

                //await remoteDevice.Verify(channelNo);

                //if (_pendingConnections.TryGetValue(deviceId, out TaskCompletionSource<RemoteDevice> tcs))
                //{
                //    // Inform clients about connection ready
                //    tcs.TrySetResult(remoteDevice);
                //    _pendingConnections.TryRemove(deviceId, out TaskCompletionSource<RemoteDevice> _);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"[{deviceId}] {ex.Message}");

                HandleDeviceInitializationError(deviceId, ex);
            }
        }

        static void HandleDeviceInitializationError(string deviceId, Exception ex)
        {
            //_connections.TryRemove(deviceId, out RemoteDevice _);
            _deviceDescriptions.TryRemove(deviceId, out DeviceDescription _);

            if (_pendingConnections.TryGetValue(deviceId, out TaskCompletionSource<RemoteDevice> tcs))
            {
                // inform clients about connection fail
                tcs.TrySetException(ex);
                _pendingConnections.TryRemove(deviceId, out TaskCompletionSource<RemoteDevice> _);
            }
        }

        internal static async Task<RemoteDevice> WaitDeviceConnection(string deviceId, int timeout)
        {
            var tcs = _pendingConnections.GetOrAdd(deviceId, (x) =>
            {
                return new TaskCompletionSource<RemoteDevice>();
            });

            try
            {
                return await tcs.Task.TimeoutAfter(timeout);
            }
            catch (TimeoutException)
            {
                tcs.TrySetException(new HideezException(HideezErrorCode.RemoteConnectionTimedOut));
            }
            finally
            {
                _pendingConnections.TryRemove(deviceId, out TaskCompletionSource<RemoteDevice> _);
            }

            return null;
        }

        public static RemoteDevice FindDevice(string deviceId)
        {
            if (_deviceDescriptions.TryGetValue(deviceId, out DeviceDescription deviceDescription))
            {
                return deviceDescription.RemoteDevice;
            }
            return null;
        }

        public static void RemoveDevice(string deviceId)
        {
            _pendingConnections.TryRemove(deviceId, out TaskCompletionSource<RemoteDevice> _);
            _deviceDescriptions.TryRemove(deviceId, out DeviceDescription _);
        }


    }
}