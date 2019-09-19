using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HES.Core.Services
{
    //todo - remove all 'static'
    public class RemoteDeviceConnectionsService : IRemoteDeviceConnectionsService
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

        const byte channelNo = 4;

        readonly IDeviceService _deviceService;
        readonly ILogger<RemoteDeviceConnectionsService> _logger;
        readonly IDataProtectionService _dataProtectionService;

        static readonly ConcurrentDictionary<string, PendingConnectionDescription> _pendingConnections
            = new ConcurrentDictionary<string, PendingConnectionDescription>();

        static readonly ConcurrentDictionary<string, RemoteDevice> _connections
            = new ConcurrentDictionary<string, RemoteDevice>();

        public RemoteDeviceConnectionsService(IDeviceService deviceService,
                                 ILogger<RemoteDeviceConnectionsService> logger,
                                 IDataProtectionService dataProtectionService)
        {
            _deviceService = deviceService;
            _logger = logger;
            _dataProtectionService = dataProtectionService;
        }

        public async void MakeConnection(string deviceId, IRemoteCommands caller)
        {
            try
            {
                var remoteDevice = new RemoteDevice(deviceId, caller, null, new SdkLogger(_logger));

                if (!_connections.TryAdd(deviceId, remoteDevice))
                    throw new Exception($"RemoteDevice already in the list of the connected devices");

                await remoteDevice.Verify(channelNo);
                await remoteDevice.Initialize();

                if (!remoteDevice.AccessLevel.IsLinkRequired)
                {
                    var prms = await GetDeviceAccessParams(deviceId);

                    await remoteDevice.Access(DateTime.UtcNow, prms.Item2, prms.Item1);

                    await remoteDevice.Initialize();

                    if (remoteDevice.AccessLevel.IsMasterKeyRequired)
                        throw new HideezException(HideezErrorCode.HesDeviceAuthorizationFailed);
                }

                if (_pendingConnections.TryGetValue(deviceId, out PendingConnectionDescription pendingConnection))
                {
                    // inform clients about connection ready
                    pendingConnection.Tcs.TrySetResult(remoteDevice);
                    _pendingConnections.TryRemove(deviceId, out PendingConnectionDescription _);
                }

                return;
            }
            catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.ERR_KEY_WRONG)
            {
                _logger.LogCritical($"[{deviceId}] {ex.Message}");
                
                //todo await ErrorMasterPassword(deviceId);

                HandleDeviceInitializationError(deviceId, ex);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);

                HandleDeviceInitializationError(deviceId, ex);
            }
        }

        static void HandleDeviceInitializationError(string deviceId, Exception ex)
        {
            _connections.TryRemove(deviceId, out RemoteDevice _);

            if (_pendingConnections.TryGetValue(deviceId, out PendingConnectionDescription descr))
            {
                // inform clients about connection fail
                descr.Tcs.TrySetException(ex);
                _pendingConnections.TryRemove(deviceId, out PendingConnectionDescription _);
            }
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

            //todo - Unprotect?
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
            finally
            {
                _pendingConnections.TryRemove(id, out PendingConnectionDescription removed);
            }

            return null;
        }

        internal static RemoteDevice FindDevice(string id)
        {
            _connections.TryGetValue(id, out RemoteDevice device);
            return device;
        }

        internal static RemoteDevice FindInitializedDevice(string id)
        {
            // a device should not be used before it initialized
            if (_pendingConnections.ContainsKey(id))
                return null;

            _connections.TryGetValue(id, out RemoteDevice device);
            return device;
        }

        internal static void RemoveDevice(string id)
        {
            _pendingConnections.TryRemove(id, out PendingConnectionDescription _);
            _connections.TryRemove(id, out RemoteDevice _);
        }
    }
}
