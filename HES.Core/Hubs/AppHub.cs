using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Services;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.Workstation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HES.Core.Hubs
{
    public class AppHub : Hub<IRemoteAppConnection>
    {
        //class DeviceDescription
        //{
        //    public IRemoteAppConnection Connection { get; }

        //    public DeviceDescription(IRemoteAppConnection connection)
        //    {
        //        Connection = connection;
        //    }
        //}

        class WorkstationDescription
        {
            public IRemoteAppConnection Connection { get; }

            public string WorkstationId { get; }

            public WorkstationDescription(IRemoteAppConnection connection, string workstationId)
            {
                Connection = connection;
                WorkstationId = workstationId;
            }
        }

        //static readonly ConcurrentDictionary<string, DeviceDescription> _deviceConnections
        //    = new ConcurrentDictionary<string, DeviceDescription>();

        static readonly ConcurrentDictionary<string, WorkstationDescription> _workstationConnections
                    = new ConcurrentDictionary<string, WorkstationDescription>();

        private readonly ConcurrentDictionary<string, TaskCompletionSource<HideezErrorInfo>> _devicesInProgress
            = new ConcurrentDictionary<string, TaskCompletionSource<HideezErrorInfo>>();

        private readonly IRemoteDeviceConnectionsService _remoteDeviceConnectionsService;
        private readonly IRemoteTaskService _remoteTaskService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkstationService _workstationService;
        private readonly IWorkstationProximityDeviceService _workstationProximityDeviceService;
        private readonly IWorkstationEventService _workstationEventService;
        private readonly IWorkstationSessionService _workstationSessionService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly ILogger<AppHub> _logger;
        private readonly IDataProtectionService _dataProtectionService;

        public AppHub(IRemoteDeviceConnectionsService remoteDeviceConnectionsService,
                      IRemoteTaskService remoteTaskService,
                      IEmployeeService employeeService,
                      IWorkstationService workstationService,
                      IWorkstationProximityDeviceService workstationProximityDeviceService,
                      IWorkstationEventService workstationEventService,
                      IWorkstationSessionService workstationSessionService,
                      IDeviceService deviceService,
                      IDeviceTaskService deviceTaskService,
                      IDeviceAccountService deviceAccountService,
                      ILogger<AppHub> logger,
                      IDataProtectionService dataProtectionService)
        {
            _remoteDeviceConnectionsService = remoteDeviceConnectionsService;
            _remoteTaskService = remoteTaskService;
            _employeeService = employeeService;
            _workstationService = workstationService;
            _workstationProximityDeviceService = workstationProximityDeviceService;
            _workstationEventService = workstationEventService;
            _workstationSessionService = workstationSessionService;
            _deviceService = deviceService;
            _deviceTaskService = deviceTaskService;
            _deviceAccountService = deviceAccountService;
            _logger = logger;
            _dataProtectionService = dataProtectionService;
        }

        #region Device

        //public override Task OnConnectedAsync()
        //{
        //    Context.Items.Add("DeviceList", new ConcurrentDictionary<string, string>());
        //    return base.OnConnectedAsync();
        //}

        public override Task OnDisconnectedAsync(Exception exception)
        {
            //// remove all device connections for this app
            //var deviceList = GetDeviceList();
            //foreach (var item in deviceList.ToArray())
            //    OnDeviceDisconnected(item.Key);

            //// Remove workstation connection info
            //var workstation = GetWorkstation();
            //if (workstation != null)
            //    OnWorkstationDisconnected(workstation.WorkstationId);

            RemoteDeviceConnectionsService.RemoveDevice(Clients.Caller);
            return base.OnDisconnectedAsync(exception);
        }

        //private ConcurrentDictionary<string, string> GetDeviceList()
        //{
        //    if (Context.Items.TryGetValue("DeviceList", out object deviceList))
        //        return deviceList as ConcurrentDictionary<string, string>;

        //    _logger.LogCritical("AppHub does not contain DeviceList collection!");

        //    var list = new ConcurrentDictionary<string, string>();
        //    Context.Items.Add("DeviceList", list);

        //    return list;
        //}

        // incoming request
        public async Task OnDeviceConnected(BleDeviceDto dto)
        {
            Debug.WriteLine($"!!!!!!!!!!!!! OnDeviceConnected {dto.DeviceSerialNo}");

            // Update Battery, Firmware, State, LastSynced         
            await _deviceService.UpdateDeviceInfoAsync(dto.DeviceSerialNo, dto.Battery, dto.FirmwareVersion, dto.IsLocked);

            RemoteDeviceConnectionsService.AddDevice(dto.DeviceSerialNo, Clients.Caller);

            //_deviceConnections.AddOrUpdate(dto.DeviceSerialNo, new DeviceDescription(Clients.Caller), (deviceMac, oldDescr) =>
            //{
            //    return new DeviceDescription(Clients.Caller);
            //});

            //var deviceList = GetDeviceList();
            //deviceList.TryAdd(dto.DeviceSerialNo, dto.DeviceSerialNo);
        }

        // incoming request
        public Task OnDeviceDisconnected(string deviceId)
        {
            Debug.WriteLine($"!!!!!!!!!!!!! OnDeviceDisconnected {deviceId}");

            if (!string.IsNullOrEmpty(deviceId))
            {
                //_deviceConnections.TryRemove(deviceId, out DeviceDescription deviceDescription);

                //var deviceList = GetDeviceList();
                //deviceList.TryRemove(deviceId, out string removed);

                RemoteDeviceConnectionsService.RemoveDevice(deviceId);
            }
            return Task.CompletedTask;
        }

        //static DeviceDescription FindDeviceDescription(string deviceId)
        //{
        //    _deviceConnections.TryGetValue(deviceId, out DeviceDescription device);
        //    return device;
        //}

        //internal static async Task<RemoteDevice> EstablishRemoteConnection(string deviceId, byte channelNo)
        //{
        //    try
        //    {
        //        RemoteDevice device = RemoteDeviceConnectionsService.FindInitializedDevice(deviceId);

        //        if (device != null)
        //        {
        //            await device.Initialize();
        //        }
        //        else
        //        {
        //            var deviceDescr = FindDeviceDescription(deviceId);
        //            if (deviceDescr == null)
        //                return null;

        //            // call Hideez Client to make remote channel
        //            await deviceDescr.Connection.EstablishRemoteDeviceConnection(deviceId, channelNo);

        //            device = await RemoteDeviceConnectionsService.WaitDeviceConnection(deviceId, channelNo, timeout: 20_000);
        //        }

        //        return device;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new HubException(ex.Message);
        //    }
        //}

        //public static bool IsDeviceConnectedToHost(string deviceId)
        //{
        //    var device = FindDeviceDescription(deviceId);
        //    return device != null;
        //}

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoByRfid(string rfid)
        {
            var device = await _deviceService
                .Query()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.RFID == rfid);

            return await GetDeviceInfo(device);
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoByMac(string mac)
        {
            var device = await _deviceService
                .Query()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MAC == mac);

            return await GetDeviceInfo(device);
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoBySerialNo(string serialNo)
        {
            var device = await _deviceService
                .Query()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == serialNo);

            return await GetDeviceInfo(device);
        }

        async Task<DeviceInfoDto> GetDeviceInfo(Device device)
        {
            if (device == null)
                return null;

            bool needUpdate = await _deviceTaskService
                .Query()
                .Where(t => t.DeviceId == device.Id)
                .AsNoTracking()
                .AnyAsync();

            var info = new DeviceInfoDto()
            {
                OwnerName = device.Employee?.FullName,
                OwnerEmail = device.Employee?.Email,
                DeviceMac = device.MAC,
                DeviceSerialNo = device.Id,
                NeedUpdate = needUpdate
            };

            return info;
        }

        public void StartUpdateRemoteDevice(string deviceId)
        {
            Debug.WriteLine($"!!!!!!!!!!!!! StartUpdateRemoteDevice {deviceId}");

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
                await UpdateRemoteDeviceWithTimeout(deviceId, tcs);
                //try
                //{
                //    Debug.WriteLine($"!!!!!!!!!!!!! StartUpdateRemoteDevice start {deviceId}");
                //    var result = await UpdateRemoteDevice(deviceId).TimeoutAfter(300_000);
                //    tcs.SetResult(result);
                //}
                //catch (TimeoutException ex)
                //{
                //    Debug.Assert(false);
                //    tcs.SetException(ex);
                //    _logger.LogCritical($"[{deviceId}] {ex.Message}");
                //}
                //catch (Exception ex)
                //{
                //    tcs.SetException(ex);
                //    _logger.LogError($"[{deviceId}] {ex.Message}");
                //}
                //finally
                //{
                //    Debug.WriteLine($"!!!!!!!!!!!!! StartUpdateRemoteDevice end {deviceId}");
                //    _devicesInProgress.TryRemove(deviceId, out TaskCompletionSource<HideezErrorInfo> _);
                //}
            });
        }

        // Incoming request
        public async Task<HideezErrorInfo> FixDevice(string deviceId)
        {
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
                Debug.WriteLine($"!!!!!!!!!!!!! FixDevice already running {deviceId}");
                return await tcs.Task;
            }

            return await UpdateRemoteDeviceWithTimeout(deviceId, tcs);
        }

        async Task<HideezErrorInfo> UpdateRemoteDeviceWithTimeout(string deviceId, TaskCompletionSource<HideezErrorInfo> tcs)
        {
            HideezErrorInfo result = HideezErrorInfo.Ok;

            try
            {
                Debug.WriteLine($"!!!!!!!!!!!!! FixDevice start {deviceId}");
                result = await UpdateRemoteDevice(deviceId).TimeoutAfter(300_000);
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

        async Task<HideezErrorInfo> UpdateRemoteDevice(string deviceId)
        {
            try
            {
                Debug.WriteLine($"!!!!!!!!!!!!! UpdateRemoteDevice {deviceId}");
                //todo
                //if (true) //conection not approved
                //throw new HideezException(HideezErrorCode.HesWorkstationNotApproved);

                //var remoteDevice = await EstablishRemoteConnection(deviceId, 4);
                var remoteDevice  = await RemoteDeviceConnectionsService.Connect(deviceId, 4);
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
                    await _remoteTaskService.ProcessTasksAsync(deviceId, TaskOperation.UnlockPin);
                    await remoteDevice.RefreshDeviceInfo();
                }

                // linking the device
                if (remoteDevice.AccessLevel.IsLinkRequired)
                {
                    // execute the Link task
                    await _remoteTaskService.ProcessTasksAsync(deviceId, TaskOperation.Link);
                    await remoteDevice.RefreshDeviceInfo();

                    // refresh MasterPassword field
                    device = await _deviceService
                        .Query()
                        .Include(d => d.DeviceAccessProfile)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.Id == deviceId);
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
                await _remoteTaskService.ProcessTasksAsync(deviceId, TaskOperation.None);

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

        #endregion

        #region Workstation

        // Incomming request
        public async Task<HideezErrorInfo> RegisterWorkstationInfo(WorkstationInfo workstationInfo)
        {
            try
            {
                var workstationDesc = _workstationConnections.AddOrUpdate(workstationInfo.Id, new WorkstationDescription(Clients.Caller, workstationInfo.Id), (workstationId, oldDescr) =>
                {
                    return new WorkstationDescription(Clients.Caller, workstationId);
                });

                Context.Items.Add("WorkstationDesc", workstationDesc);

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

        async Task OnWorkstationConnected(string workstationId)
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

        Task OnWorkstationDisconnected(string workstationId)
        {
            _logger.LogDebug($"[{workstationId}] disconnected");

            _workstationConnections.TryRemove(workstationId, out WorkstationDescription _);
            Context.Items.Remove("WorkstationDesc");

            return Task.CompletedTask;
        }

        //private WorkstationDescription GetWorkstation()
        //{
        //    if (Context.Items.TryGetValue("WorkstationDesc", out object workstation))
        //        return workstation as WorkstationDescription;
        //    else
        //        return null;
        //}

        static WorkstationDescription FindWorkstationDescription(string workstationId)
        {
            _workstationConnections.TryGetValue(workstationId, out WorkstationDescription workstation);
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
                var workstation = FindWorkstationDescription(workstationId);
                if (workstation != null)
                {
                    await workstation.Connection.UpdateProximitySettings(deviceProximitySettings);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }

        public static async Task UpdateRfidIndicatorState(string workstationId, bool isEnabled)
        {
            try
            {
                var workstation = FindWorkstationDescription(workstationId);
                if (workstation != null)
                {
                    await workstation.Connection.UpdateRFIDIndicatorState(isEnabled);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }

        #endregion

        #region Audit

        // Incomming request
        public async Task<HideezErrorInfo> SaveClientEvents(WorkstationEventDto[] events)
        {
            try
            {
                if (events == null)
                    throw new ArgumentNullException(nameof(events));

                _logger.LogDebug($"[{events[0].WorkstationId}] Sent events: {string.Join("; ", events.Select(s => s.EventId))}");
                // todo: ignore not approved workstation

                // Events that duplicate ID of other events are ignored
                events = events.GroupBy(e => e.Id).Select(s => s.First()).ToArray();

                // Filter out from incomming events all those who share ID with events saved in database 
                var filtered = events.Where(e => !_workstationEventService.Query().Any(we => we.Id == e.Id)).ToList(); //TODO move to Async

                // Convert from SDK WorkstationEvent to HES WorkstationEvent
                List<WorkstationEvent> converted = new List<WorkstationEvent>();
                foreach (var dto in filtered)
                {
                    var convertedEvent =
                        new WorkstationEvent()
                        {
                            Id = dto.Id,
                            Date = dto.Date,
                            EventId = dto.EventId,
                            SeverityId = dto.SeverityId,
                            Note = dto.Note,
                            WorkstationId = dto.WorkstationId,
                            UserSession = dto.UserSession,
                            DeviceId = dto.DeviceId,
                        };

                    if (!string.IsNullOrWhiteSpace(dto.AccountName) && !string.IsNullOrWhiteSpace(dto.AccountLogin))
                    {
                        convertedEvent.DeviceAccount = await _deviceAccountService
                            .Query()
                            .Where(d => d.Name == dto.AccountName
                                     && d.Login == dto.AccountLogin
                                     && d.DeviceId == dto.DeviceId)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();
                    }
                    converted.Add(convertedEvent);
                }


                var addedEvents = await _workstationEventService.AddEventsRangeAsync(converted);

                var authEventsOnly = converted.Where(e => e.EventId == WorkstationEventType.ComputerLock
                    || e.EventId == WorkstationEventType.ComputerLogoff
                    || e.EventId == WorkstationEventType.ComputerLogon
                    || e.EventId == WorkstationEventType.ComputerUnlock).ToArray();

                if (authEventsOnly.Length > 0)
                    await _workstationSessionService.UpdateWorkstationSessionsAsync(authEventsOnly);

                return HideezErrorInfo.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return new HideezErrorInfo(ex);
            }
        }

        #endregion
    }
}