using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Utils;
using Hideez.SDK.Communication.Workstation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Hubs
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

        static readonly ConcurrentDictionary<string, DeviceDescription> _deviceConnections
            = new ConcurrentDictionary<string, DeviceDescription>();

        static readonly ConcurrentDictionary<string, WorkstationDescription> _workstationConnections
                    = new ConcurrentDictionary<string, WorkstationDescription>();

        private readonly IRemoteTaskService _remoteTaskService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkstationService _workstationService;
        private readonly IWorkstationEventService _workstationEventService;
        private readonly IWorkstationSessionService _workstationSessionService;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly ILogger<AppHub> _logger;

        public AppHub(IRemoteTaskService remoteTaskService,
                      IEmployeeService employeeService,
                      IWorkstationService workstationService,
                      IWorkstationEventService workstationEventService,
                      IWorkstationSessionService workstationSessionService,
                      IDeviceService deviceService,
                      IDeviceAccountService deviceAccountService,
                      ILogger<AppHub> logger)
        {
            _remoteTaskService = remoteTaskService;
            _employeeService = employeeService;
            _workstationService = workstationService;
            _workstationEventService = workstationEventService;
            _workstationSessionService = workstationSessionService;
            _deviceService = deviceService;
            _deviceAccountService = deviceAccountService;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            Context.Items.Add("DeviceList", new ConcurrentDictionary<string, string>());
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // remove all device connections for this app
            var deviceList = GetDeviceList();
            foreach (var item in deviceList.ToArray())
                OnDeviceDisconnected(item.Key);

            // Remove workstation connection info
            var workstation = GetWorkstation();
            if (workstation != null)
                OnWorkstationDisconnected(workstation.WorkstationId);

            return base.OnDisconnectedAsync(exception);
        }

        private ConcurrentDictionary<string, string> GetDeviceList()
        {
            if (Context.Items.TryGetValue("DeviceList", out object deviceList))
                return deviceList as ConcurrentDictionary<string, string>;

            _logger.LogCritical("AppHub does not contain DeviceList collection!");

            var list = new ConcurrentDictionary<string, string>();
            Context.Items.Add("DeviceList", list);

            return list;
        }

        public async Task OnDeviceConnected(BleDeviceDto dto)
        {
            // Update Battery, Firmware, State, LastSynced         
            await _deviceService.UpdateDevicePropAsync(dto.DeviceSerialNo, dto.Battery, dto.FirmwareVersion, dto.IsLocked);

            _deviceConnections.AddOrUpdate(dto.DeviceSerialNo, new DeviceDescription(Clients.Caller), (deviceMac, oldDescr) =>
            {
                return new DeviceDescription(Clients.Caller);
            });

            var deviceList = GetDeviceList();
            deviceList.TryAdd(dto.DeviceSerialNo, dto.DeviceSerialNo);

            _remoteTaskService.StartTaskProcessing(dto.DeviceSerialNo);
        }

        public Task OnDeviceDisconnected(string deviceId)
        {
            _deviceConnections.TryRemove(deviceId, out DeviceDescription deviceDescription);

            var deviceList = GetDeviceList();
            deviceList.TryRemove(deviceId, out string removed);

            DeviceHub.RemoveDevice(deviceId);

            return Task.CompletedTask;
        }

        static DeviceDescription FindDeviceDescription(string deviceId)
        {
            _deviceConnections.TryGetValue(deviceId, out DeviceDescription device);
            return device;
        }

        public static async Task<RemoteDevice> EstablishRemoteConnection(string deviceId, byte channelNo)
        {
            try
            {
                var device = DeviceHub.FindDevice(deviceId);
                if (device != null)
                    return device;

                var deviceDescr = FindDeviceDescription(deviceId);
                if (deviceDescr == null)
                    return null;

                await deviceDescr.Connection.EstablishRemoteDeviceConnection(deviceId, channelNo);

                var remoteDevice = await DeviceHub.WaitDeviceConnection(deviceId, timeout: 10000);

                if (remoteDevice != null)
                    await remoteDevice.WaitAuthentication(timeout: 10000);

                await remoteDevice.Initialize();

                return remoteDevice;
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }

        public static bool IsDeviceConnectedToHost(string deviceId)
        {
            var device = FindDeviceDescription(deviceId);
            return device != null;
        }

        public static bool IsWorkstationOnline(string workstationId)
        {
            //todo
            var r = new Random().Next(100) < 50 ? true : false;
            return r;
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoByRfid(string rfid)
        {
            var device = await _employeeService
                .DeviceQuery()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.RFID == rfid);

            return await GetDeviceInfo(device);
        }

        // Incomming request
        public async Task<DeviceInfoDto> GetInfoByMac(string mac)
        {
            var device = await _employeeService
                .DeviceQuery()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MAC == mac);

            return await GetDeviceInfo(device);
        }

        private async Task<DeviceInfoDto> GetDeviceInfo(Device device)
        {
            if (device == null)
                return null;

            bool needUpdate = await _employeeService
                .DeviceTaskQuery()
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

        // Incomming request
        public async Task RegisterWorkstationInfo(WorkstationInfo workstationInfo)
        {
            var workstationDesc = _workstationConnections.AddOrUpdate(workstationInfo.Id, new WorkstationDescription(Clients.Caller, workstationInfo.Id), (workstationId, oldDescr) =>
            {
                return new WorkstationDescription(Clients.Caller, workstationId);
            });

            Context.Items.Add("WorkstationDesc", workstationDesc);

            if (await _workstationService.ExistAsync(w => w.Id == workstationInfo.Id))
            {
                // Workstation exists, update its information
                await _workstationService.UpdateWorkstationAsync(workstationInfo);
            }
            else
            {
                // Workstation does not exist in DB or its name + domain was changed
                // Create new unapproved workstation      
                await _workstationService.AddWorkstationAsync(workstationInfo);
            }

            await OnWorkstationConnected(workstationInfo.Id);
        }

        // Incomming request
        public async Task FixDevice(string deviceId)
        {
            try
            {
                var remoteDevice = await EstablishRemoteConnection(deviceId, 4);
                if (remoteDevice == null)
                    return;

                var device = await _deviceService
                    .Query()
                    .Include(d => d.DeviceAccessProfile)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == deviceId);

                var key = ConvertUtils.HexStringToBytes(device.MasterPassword);

                // Getting device info
                await remoteDevice.Initialize();

                // Access 
                if (remoteDevice.IsMasterKeyRequired)
                {
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

                    await remoteDevice.Access(DateTime.UtcNow, key, accessParams);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task OnWorkstationConnected(string workstationId)
        {
            try
            {
                var unlockerSettingsInfo = await _workstationService.GetWorkstationUnlockerSettingsInfoAsync(workstationId);

                await UpdateUnlockerSettings(workstationId, unlockerSettingsInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private Task OnWorkstationDisconnected(string workstationId)
        {
            _workstationConnections.TryRemove(workstationId, out WorkstationDescription workstation);

            return Task.CompletedTask;
        }

        private WorkstationDescription GetWorkstation()
        {
            if (Context.Items.TryGetValue("WorkstationDesc", out object workstation))
                return workstation as WorkstationDescription;
            else
                return null;
        }

        private static WorkstationDescription FindWorkstationDescription(string workstationId)
        {
            _workstationConnections.TryGetValue(workstationId, out WorkstationDescription workstation);
            return workstation;
        }

        public static bool IsWorkstationConnectedToHost(string workstationId)
        {
            var workstation = FindWorkstationDescription(workstationId);
            return workstation != null;
        }

        public static async Task UpdateUnlockerSettings(string workstationId, UnlockerSettingsInfo unlockerSettingsInfo)
        {
            try
            {
                var workstation = FindWorkstationDescription(workstationId);
                if (workstation != null)
                {
                    await workstation.Connection.UpdateUnlockerSettings(unlockerSettingsInfo);
                }
            }
            catch (Exception ex)
            {
                throw new HubException(ex.Message);
            }
        }

        public async Task<bool> SaveClientEvents(WorkstationEventDto[] events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            // todo: ignore not approved workstation

            // Events that duplicate ID of other events are ignored
            events = events.GroupBy(e => e.Id).Select(s => s.First()).ToArray();

            // Filter out from incomming events all those who share ID with events saved in database 
            var filtered = events.Where(e => !_workstationEventService.WorkstationEventQuery().Any(we => we.Id == e.Id)).ToList(); //TODO move to Async

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

            try
            {
                var addedEvents = await _workstationEventService.AddEventsRangeAsync(converted);

                var authEventsOnly = converted.Where(e => e.EventId == WorkstationEventType.ComputerLock
                || e.EventId == WorkstationEventType.ComputerLogoff
                || e.EventId == WorkstationEventType.ComputerLogon
                || e.EventId == WorkstationEventType.ComputerUnlock).ToArray();

                if (authEventsOnly.Length > 0)
                    await _workstationSessionService.UpdateWorkstationSessionsAsync(authEventsOnly);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }

            return true;
        }
    }
}