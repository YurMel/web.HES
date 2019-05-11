using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Remote;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HES.Core.Services
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

        static readonly ConcurrentDictionary<string, DeviceDescription> _deviceConnections
            = new ConcurrentDictionary<string, DeviceDescription>();

        private readonly IRemoteTaskService _remoteTaskService;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<AppHub> _logger;

        public AppHub(IRemoteTaskService remoteTaskService, IEmployeeService employeeService, ILogger<AppHub> logger)
        {
            _remoteTaskService = remoteTaskService;
            _employeeService = employeeService;
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            Context.Items.Add("DeviceMacs", new ConcurrentDictionary<string, string>());

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            // remove all device connections for this app
            var deviceList = GetDeviceMacList();
            foreach (var item in deviceList.ToArray())
            {
                OnDeviceDisconnected(item.Key);
            }

            return base.OnDisconnectedAsync(exception);
        }

        ConcurrentDictionary<string, string> GetDeviceMacList()
        {
            if (Context.Items.TryGetValue("DeviceMacs", out object deviceMacs))
                return deviceMacs as ConcurrentDictionary<string, string>;

            Debug.Assert(false);

            var list = new ConcurrentDictionary<string, string>();
            Context.Items.Add("DeviceMacs", list);

            return list;
        }

        public async Task OnDeviceConnected(string mac)
        {
            _deviceConnections.AddOrUpdate(mac, new DeviceDescription(Clients.Caller), (deviceMac, oldDescr) =>
            {
                return new DeviceDescription(Clients.Caller);
            });

            var deviceList = GetDeviceMacList();
            deviceList.TryAdd(mac, mac);

            await _remoteTaskService.ExecuteRemoteTasks(mac);
        }

        public Task OnDeviceDisconnected(string mac)
        {
            _deviceConnections.TryRemove(mac, out DeviceDescription deviceDescription);

            var deviceList = GetDeviceMacList();
            deviceList.TryRemove(mac, out string removed);

            return Task.CompletedTask;
        }

        static DeviceDescription FindDeviceDescription(string id)
        {
            _deviceConnections.TryGetValue(id, out DeviceDescription device);
            return device;
        }

        public static async Task<RemoteDevice> EstablishRemoteConnection(string id, byte channelNo)
        {
            try
            {
                var device = DeviceHub.FindDevice(id);
                if (device != null)
                    return device;

                var deviceDescr = FindDeviceDescription(id);
                if (deviceDescr == null)
                    throw new HideezException(HideezErrorCode.DeviceNotConnectedToAnyHost);

                await deviceDescr.Connection.EstablishRemoteDeviceConnection(id, channelNo);

                var remoteDevice = await DeviceHub.WaitDeviceConnection(id, timeout: 10000);

                if (remoteDevice != null)
                    await remoteDevice.WaitAuthentication(timeout: 10000);

                return remoteDevice;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw new HubException(ex.Message);
            }
        }

        public static bool IsDeviceConnectedToHost(string id)
        {
            var device = FindDeviceDescription(id);
            return device != null;
        }

        public async Task<UserInfo> GetInfoByRfid(string rfid)
        {
            var device = await _employeeService.DeviceQuery().Include(d => d.Employee).FirstOrDefaultAsync(d => d.RFID == rfid);
            if (device == null)
            {
                return null;
            }

            var primaryAccount = await _employeeService.DeviceAccountQuery().FirstOrDefaultAsync(d => d.Id == device.PrimaryAccountId);

            bool needUpdatePrimaryAccount = false;
            if (primaryAccount != null)
            {
                needUpdatePrimaryAccount = await _employeeService.DeviceTaskQuery().Where(t => t.DeviceAccountId == primaryAccount.Id).AnyAsync();
            }

            var info = new UserInfo()
            {
                Name = device?.Employee?.FullName,
                DeviceMac = device?.MAC,
                DeviceSerialNo = device?.Id,
                PrimaryAccountLogin = primaryAccount?.Login,
                IdFromDevice = primaryAccount?.IdFromDevice,
                NeedUpdatePrimaryAccount = needUpdatePrimaryAccount,
                DeviceMasterPassword = device?.MasterPassword
            };

            return info;
        }
    }
}