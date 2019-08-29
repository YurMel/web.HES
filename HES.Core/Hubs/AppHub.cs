using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Command;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Workstation;
using Hideez.SDK.Communication.WorkstationEvents;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
//using WorkstationEvent = HES.Core.Entities.WorkstationEvent;

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
        private readonly ILogger<AppHub> _logger;

        public AppHub(IRemoteTaskService remoteTaskService, IEmployeeService employeeService, IWorkstationService workstationService, IWorkstationEventService workstationEventService, IWorkstationSessionService workstationSessionService, ILogger<AppHub> logger)
        {
            _remoteTaskService = remoteTaskService;
            _employeeService = employeeService;
            _workstationService = workstationService;
            _workstationEventService = workstationEventService;
            _workstationSessionService = workstationSessionService;
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

        public Task OnDeviceConnected_v2(string deviceId, byte battery, string firmwareVersion, bool isLocked)
        {
            //todo - save battery, isLocked and firmwareVersion to the DB

            return OnDeviceConnected(deviceId);
        }

        public Task OnDeviceConnected(string deviceId)
        {
            _deviceConnections.AddOrUpdate(deviceId, new DeviceDescription(Clients.Caller), (deviceMac, oldDescr) =>
            {
                return new DeviceDescription(Clients.Caller);
            });

            var deviceList = GetDeviceList();
            deviceList.TryAdd(deviceId, deviceId);

            _remoteTaskService.StartTaskProcessing(deviceId);

            return Task.CompletedTask;
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
            var r = new Random().Next(100) < 50 ? true : false;
            return r;
        }

        // Incomming request
        public async Task<UserInfo> GetInfoByRfid(string rfid)
        {
            var device = await _employeeService
                .DeviceQuery()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.RFID == rfid);

            if (device == null)
                return null;

            var primaryAccount = await _employeeService
                .DeviceAccountQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == device.PrimaryAccountId);

            bool needUpdatePrimaryAccount = false;
            if (primaryAccount != null)
            {
                needUpdatePrimaryAccount = await _employeeService
                    .DeviceTaskQuery()
                    .Where(t => t.DeviceAccountId == primaryAccount.Id)
                    .AsNoTracking()
                    .AnyAsync();
            }

            var info = new UserInfo()
            {
                Name = device.Employee?.FullName,
                DeviceMac = device.MAC,
                DeviceSerialNo = device.Id,
                PrimaryAccountLogin = primaryAccount?.Login,
                IdFromDevice = primaryAccount?.IdFromDevice ?? 0,
                NeedUpdatePrimaryAccount = needUpdatePrimaryAccount,
                DeviceMasterPassword = device.MasterPassword
            };

            return info;
        }

        // Incomming request
        public async Task<UserInfo> GetInfoByMac(string mac)
        {
            var device = await _employeeService
                .DeviceQuery()
                .Include(d => d.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MAC == mac);

            if (device == null)
                return null;

            var primaryAccount = await _employeeService
                .DeviceAccountQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == device.PrimaryAccountId);

            bool needUpdatePrimaryAccount = false;
            if (primaryAccount != null)
            {
                needUpdatePrimaryAccount = await _employeeService
                    .DeviceTaskQuery()
                    .Where(t => t.DeviceAccountId == primaryAccount.Id)
                    .AsNoTracking()
                    .AnyAsync();
            }

            var info = new UserInfo()
            {
                Name = device.Employee?.FullName,
                DeviceMac = device.MAC,
                DeviceSerialNo = device.Id,
                PrimaryAccountLogin = primaryAccount?.Login,
                IdFromDevice = primaryAccount?.IdFromDevice ?? 0,
                NeedUpdatePrimaryAccount = needUpdatePrimaryAccount,
                DeviceMasterPassword = device.MasterPassword
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


                var key = Encoding.UTF8.GetBytes("passphrase"); //todo - read from DB

                // getting device info
                await remoteDevice.Initialize();

                // access 
                if (remoteDevice.IsMasterKeyRequired)
                {
                    var accessParams = new AccessParams()
                    {
                        MasterKey_Bond = true,
                        MasterKey_Connect = false,
                        MasterKey_Link = false,
                        MasterKey_Channel = false,

                        Button_Bond = false,
                        Button_Connect = false,
                        Button_Link = true,
                        Button_Channel = true,

                        Pin_Bond = false,
                        Pin_Connect = true,
                        Pin_Link = false,
                        Pin_Channel = false,

                        PinMinLength = 4,
                        PinMaxTries = 3,
                        MasterKeyExpirationPeriod = 24 * 60 * 60,
                        PinExpirationPeriod = 15 * 60,
                        ButtonExpirationPeriod = 15,
                    }; //todo - read AccessParams from the DB

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

        public async Task<bool> SaveClientEvents(SdkWorkstationEvent[] events)
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
            foreach (var other in filtered)
            {
                var convertedEvent =
                    new WorkstationEvent()
                    {
                        Id = other.Id,
                        Date = other.Date,
                        EventId = other.EventId,
                        SeverityId = other.Severity,
                        Note = other.Note,
                        WorkstationId = other.WorkstationId,
                        UserSession = other.UserSession,
                        DeviceId = other.DeviceId,
                    };

                if (!string.IsNullOrWhiteSpace(other.AccountName)
                    && !string.IsNullOrWhiteSpace(other.AccountLogin))
                {
                    convertedEvent.DeviceAccount = new DeviceAccount() // todo: querry DB for the account
                    {
                        Name = other.AccountName,
                        Login = other.AccountLogin,
                    };
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
            }

            return true;
        }
    }
}