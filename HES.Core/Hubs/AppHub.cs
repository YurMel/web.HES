using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Workstation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<AppHub> _logger;

        public AppHub(IRemoteTaskService remoteTaskService, IEmployeeService employeeService, IWorkstationService workstationService, ILogger<AppHub> logger)
        {
            _remoteTaskService = remoteTaskService;
            _employeeService = employeeService;
            _workstationService = workstationService;
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

        ConcurrentDictionary<string, string> GetDeviceList()
        {
            if (Context.Items.TryGetValue("DeviceList", out object deviceList))
                return deviceList as ConcurrentDictionary<string, string>;

            _logger.LogCritical("AppHub does not contain DeviceList collection!");

            var list = new ConcurrentDictionary<string, string>();
            Context.Items.Add("DeviceList", list);

            return list;
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

            // Todo: might be not the best way or place to do this
            if (_workstationService.Exist(w => w.Id == workstationInfo.Id))
            {
                // Workstation exists, update its information
                await _workstationService.UpdateClientVersionAsync(workstationInfo.Id, workstationInfo.AppVersion);
                await _workstationService.UpdateOsAsync(workstationInfo.Id, workstationInfo.OsName);
                await _workstationService.UpdateIpAsync(workstationInfo.Id, workstationInfo.IP);
                await _workstationService.UpdateLastSeenAsync(workstationInfo.Id);
            }
            else 
            {
                // Workstation does not exist in DB or its name + domain was changed
                // Create new unapproved workstation
                var workstation = new Workstation()
                {
                    Id = workstationInfo.Id,
                    Name = workstationInfo.MachineName,
                    //Domain = workstationInfo.Domain,
                    OS = workstationInfo.OsName,
                    ClientVersion = workstationInfo.AppVersion,
                    IP = workstationInfo.IP,
                    LastSeen = DateTime.UtcNow,
                };
                await _workstationService.AddWorkstationAsync(workstation);
            }

            await OnWorkstationConnected(workstationInfo.Id);
        }

        async Task OnWorkstationConnected(string workstationId)
        {
            try
            {
                var unlockerSettingsInfo = _workstationService.GetWorkstationUnlockerSettingsInfo(workstationId);

                await UpdateUnlockerSettings(workstationId, unlockerSettingsInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        Task OnWorkstationDisconnected(string workstationId)
        {
            _workstationConnections.TryRemove(workstationId, out WorkstationDescription workstation);

            return Task.CompletedTask;
        }

        WorkstationDescription GetWorkstation()
        {
            if (Context.Items.TryGetValue("WorkstationDesc", out object workstation))
                return workstation as WorkstationDescription;
            else
                return null;
        }

        static WorkstationDescription FindWorkstationDescription(string workstationId)
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
    }
}