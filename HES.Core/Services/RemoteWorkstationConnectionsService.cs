using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.Workstation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HES.Core.Services
{
    public class RemoteWorkstationConnectionsService : IRemoteWorkstationConnectionsService
    {
        static readonly ConcurrentDictionary<string, IRemoteAppConnection> _workstationConnections
                    = new ConcurrentDictionary<string, IRemoteAppConnection>();

        static readonly ConcurrentDictionary<string, TaskCompletionSource<HideezErrorInfo>> _devicesInProgress
            = new ConcurrentDictionary<string, TaskCompletionSource<HideezErrorInfo>>();

        readonly IRemoteTaskService _remoteTaskService;
        readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        readonly IEmployeeService _employeeService;
        readonly IWorkstationService _workstationService;
        readonly IWorkstationProximityDeviceService _workstationProximityDeviceService;
        readonly IDeviceService _deviceService;
        readonly IDataProtectionService _dataProtectionService;
        readonly IWorkstationSessionService _workstationSessionService;
        readonly ILogger<RemoteWorkstationConnectionsService> _logger;

        public RemoteWorkstationConnectionsService(
                      IRemoteTaskService remoteTaskService,
                      IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IEmployeeService employeeService,
                      IWorkstationService workstationService,
                      IWorkstationProximityDeviceService workstationProximityDeviceService,
                      IDeviceService deviceService,
                      IDataProtectionService dataProtectionService,
                      IWorkstationSessionService workstationSessionService,
                      ILogger<RemoteWorkstationConnectionsService> logger)
        {
            _remoteTaskService = remoteTaskService;
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _employeeService = employeeService;
            _workstationService = workstationService;
            _workstationProximityDeviceService = workstationProximityDeviceService;
            _deviceService = deviceService;
            _dataProtectionService = dataProtectionService;
            _workstationSessionService = workstationSessionService;
            _logger = logger;
        }

        #region Device

        public void StartUpdateRemoteDevice(IList<string> devicesId)
        {
            foreach (var device in devicesId)
            {
                StartUpdateRemoteDevice(device);
            }
        }

        public void StartUpdateRemoteDevice(string deviceId)
        {
            Debug.WriteLine($"!!!!!!!!!!!!! StartUpdateRemoteDevice {deviceId}");
            if (deviceId == null)
                throw new ArgumentNullException(nameof(deviceId));

            var isNew = false;

            var tcs = _devicesInProgress.GetOrAdd(deviceId, (x) =>
            {
                isNew = true;
                return new TaskCompletionSource<HideezErrorInfo>();
            });

            if (!isNew)
            {
                Debug.WriteLine($"!!!!!!!!!!!!! StartUpdateRemoteDevice already running {deviceId}");
                return;
            }

            Task.Run(async () =>
            {
                await UpdateRemoteDeviceWithTimeout(deviceId, tcs, workstationId: null);
            });
        }

        public async Task<HideezErrorInfo> UpdateRemoteDeviceAsync(string deviceId, string workstationId)
        {
            Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDeviceAsync start {deviceId}");
            if (deviceId == null)
                throw new ArgumentNullException(nameof(deviceId));

            var isNew = false;
            var tcs = _devicesInProgress.GetOrAdd(deviceId, (x) =>
            {
                isNew = true;
                return new TaskCompletionSource<HideezErrorInfo>();
            });

            if (!isNew)
            {
                Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDeviceAsync already running {deviceId}");
                return await tcs.Task;
            }

            return await UpdateRemoteDeviceWithTimeout(deviceId, tcs, workstationId);
        }

        private async Task<HideezErrorInfo> UpdateRemoteDeviceWithTimeout(string deviceId, TaskCompletionSource<HideezErrorInfo> tcs, string workstationId)
        {
            HideezErrorInfo result = HideezErrorInfo.Ok;

            try
            {
                Debug.WriteLine($"!!!!!!!!!!!!! FixDevice start {deviceId}");
                result = await UpdateRemoteDevice(deviceId, workstationId).TimeoutAfter(300_000);
                tcs.SetResult(result);
            }
            catch (TimeoutException ex)
            {
                Debug.Assert(false);
                tcs.SetException(ex);
                _logger.LogCritical($"[{deviceId}] {ex.Message}");
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
                _logger.LogError($"[{deviceId}] {ex.Message}");
            }
            finally
            {
                Debug.WriteLine($"!!!!!!!!!!!!! FixDevice end {deviceId}, {result.Code}");
                _devicesInProgress.TryRemove(deviceId, out TaskCompletionSource<HideezErrorInfo> _);
            }

            return result;
        }

        private async Task<HideezErrorInfo> UpdateRemoteDevice(string deviceId, string workstationId)
        {
            try
            {
                Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDevice {deviceId}");
                //todo
                //if (true) //conection not approved
                //throw new HideezException(HideezErrorCode.HesWorkstationNotApproved);

                var remoteDevice = await _remoteDeviceConnectionsService
                    .ConnectDevice(deviceId, workstationId)
                    .TimeoutAfter(30_000);

                if (remoteDevice == null)
                    throw new HideezException(HideezErrorCode.HesFailedEstablishRemoteDeviceConnection);

                var device = await _deviceService
                    .Query()
                    .Include(d => d.DeviceAccessProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == deviceId);

                if (device == null)
                    throw new HideezException(HideezErrorCode.HesDeviceNotFound);

                // Getting device info
                await remoteDevice.Initialize();

                // unlocking the device 
                if (remoteDevice.AccessLevel.IsLocked)
                {
                    // execute the UnlockPin task
                    await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.UnlockPin);
                    await remoteDevice.RefreshDeviceInfo();
                }

                // linking the device
                if (remoteDevice.AccessLevel.IsLinkRequired)
                {
                    // execute the Link task
                    await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.Link);
                    await remoteDevice.RefreshDeviceInfo();

                    // refresh MasterPassword field
                    device = await _deviceService
                        .Query()
                        .Include(d => d.DeviceAccessProfile)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.Id == deviceId);

                    if (remoteDevice.AccessLevel.IsLinkRequired)
                    {
                        // we have tried to link the device with no success. Just in case clearing all device's tasks and linkings
                        await _employeeService.HandlingMasterPasswordErrorAsync(deviceId);
                        throw new HideezException(HideezErrorCode.HesDeviceNotAssignedToAnyUser);
                    }
                }

                // Access 
                if (device.DeviceAccessProfile == null)
                    throw new HideezException(HideezErrorCode.HesEmptyDeviceAccessProfile);

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

                if (string.IsNullOrWhiteSpace(device.MasterPassword))
                    throw new HideezException(HideezErrorCode.HesEmptyMasterKey);

                var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Unprotect(device.MasterPassword));

                await remoteDevice.Access(DateTime.UtcNow, key, accessParams);

                //todo - remove this block when FW will be fixed
                {
                    await remoteDevice.Initialize();
                    if (remoteDevice.AccessLevel.IsMasterKeyRequired)
                    {
                        await remoteDevice.Access(DateTime.UtcNow, key, accessParams);
                        await remoteDevice.Initialize();
                    }
                }

                // all tasks processing
                var res = await _remoteTaskService.ExecuteRemoteTasks(deviceId, remoteDevice, TaskOperation.None);
                if (res != HideezErrorCode.Ok)
                    throw new HideezException(res);

                Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDevice OK");
                return HideezErrorInfo.Ok;
            }
            catch (HideezException ex) when (ex.ErrorCode == HideezErrorCode.ERR_KEY_WRONG)
            {
                Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDevice ERROR {ex.Message}");
                _logger.LogCritical($"[{deviceId}] {ex.Message}");
                await _employeeService.HandlingMasterPasswordErrorAsync(deviceId);
                return new HideezErrorInfo(ex);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDevice ERROR {ex.Message}");
                _logger.LogError($"[{deviceId}] {ex.Message}");
                return new HideezErrorInfo(ex);
            }
        }

        #endregion Device

        #region Workstation

        public IRemoteAppConnection GetOrAddWorkstationInfo(string workstationId, IRemoteAppConnection remoteAppConnection)
        {
            var workstationDescr = _workstationConnections.GetOrAdd(workstationId, (x) =>
            {
                return remoteAppConnection;
            });
            return workstationDescr;
        }

        public async Task<HideezErrorInfo> RegisterWorkstationInfo(IRemoteAppConnection remoteAppConnection, WorkstationInfo workstationInfo)
        {
            try
            {
                _workstationConnections.AddOrUpdate(workstationInfo.Id, remoteAppConnection, (id, oldConnection) =>
                {
                    return remoteAppConnection;
                });

                if (await _workstationService.ExistAsync(w => w.Id == workstationInfo.Id))
                {
                    // Workstation exists, update its information
                    await _workstationService.UpdateWorkstationInfoAsync(workstationInfo);
                }
                else
                {
                    // Workstation does not exist in DB or its name + domain was changed
                    // Create new unapproved workstation      
                    await _workstationService.AddWorkstationAsync(workstationInfo);
                    _logger.LogInformation($"New workstation {workstationInfo.MachineName} was added");
                }

                await OnWorkstationConnected(workstationInfo.Id);

                return HideezErrorInfo.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{workstationInfo.MachineName}] {ex.Message}");
                return new HideezErrorInfo(ex);
            }
        }

        private async Task OnWorkstationConnected(string workstationId)
        {
            try
            {
                _logger.LogDebug($"[{workstationId}] connected");
                await _workstationProximityDeviceService.UpdateProximitySettingsAsync(workstationId);

                await _workstationService.UpdateRfidStateAsync(workstationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task OnWorkstationDisconnected(string workstationId)
        {
            _logger.LogDebug($"[{workstationId}] disconnected");

            _workstationConnections.TryRemove(workstationId, out IRemoteAppConnection _);

            await _workstationSessionService.CloseSessionAsync(workstationId);
        }

        private static IRemoteAppConnection FindWorkstationConnection(string workstationId)
        {
            _workstationConnections.TryGetValue(workstationId, out IRemoteAppConnection workstation);
            return workstation;
        }

        public static bool IsWorkstationConnectedToServer(string workstationId)
        {
            return _workstationConnections.ContainsKey(workstationId);
        }

        public static async Task UpdateProximitySettings(string workstationId, IReadOnlyList<DeviceProximitySettingsDto> deviceProximitySettings)
        {
            try
            {
                var workstationConnection = FindWorkstationConnection(workstationId);
                if (workstationConnection != null)
                {
                    await workstationConnection.UpdateProximitySettings(deviceProximitySettings);
                }
            }
            catch (Exception ex)
            {
                //todo- why HubException ?
                throw new HubException(ex.Message);
            }
        }

        public static async Task UpdateRfidIndicatorState(string workstationId, bool isEnabled)
        {
            try
            {
                var workstationConnection = FindWorkstationConnection(workstationId);
                if (workstationConnection != null)
                {
                    await workstationConnection.UpdateRFIDIndicatorState(isEnabled);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }

        #endregion
    }
}
