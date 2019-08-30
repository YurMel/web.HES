using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.WorkstationEvents;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceService : IDeviceService
    {
        class MyHideezDevice
        {
            public string Id { get; set; }
            public string MAC { get; set; }
            public string ManufacturerUserId { get; set; }
            public string RFID { get; set; }
            public string Model { get; set; }
            public string BootLoaderVersion { get; set; }
            public DateTime Manufactured { get; set; }
            public string CpuSerialNo { get; set; }
            public byte[] DeviceKey { get; set; }
            public int? BleDeviceBatchId { get; set; }
            public string RegisteredUserId { get; set; }
        }

        private readonly IAesCryptography _aes;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;
        private readonly IDeviceAccessProfilesService _deviceAccessProfilesService;
        private readonly IWorkstationEventService _workstationEventService;
        private readonly IRemoteTaskService _remoteTaskService;

        public DeviceService(IAesCryptography aes,
                             IAsyncRepository<Device> deviceRepository,
                             IAsyncRepository<DeviceTask> deviceTaskRepository,
                             IDeviceAccessProfilesService deviceAccessProfilesService,
                             IWorkstationEventService workstationEventService,
                             IRemoteTaskService remoteTaskService)
        {
            _aes = aes;
            _deviceRepository = deviceRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _deviceAccessProfilesService = deviceAccessProfilesService;
            _workstationEventService = workstationEventService;
            _remoteTaskService = remoteTaskService;
        }

        public IQueryable<Device> DeviceQuery()
        {
            return _deviceRepository.Query();
        }

        public async Task<Device> DeviceGetByIdAsync(dynamic id)
        {
            return await _deviceRepository.GetByIdAsync(id);
        }

        public async Task<(IList<Device> devicesExists, IList<Device> devicesImported, string message)> ImportDevices(string key, byte[] fileContent)
        {
            IList<Device> devicesExists = null;
            IList<Device> devicesImported = null;
            string message = null;

            var objects = _aes.DecryptObject<List<MyHideezDevice>>(fileContent, Encoding.Unicode.GetBytes(key));
            if (objects.Count > 0)
            {
                // Get all exists devices
                var isExist = await _deviceRepository.Query().Where(d => objects.Select(o => o.Id).Contains(d.Id)).ToListAsync();
                if (isExist.Count > 0)
                {
                    devicesExists = isExist;
                }
                // Devices to import
                var toImport = objects.Where(z => !isExist.Select(m => m.Id).Contains(z.Id)).Select(d => new Device()
                {
                    Id = d.Id,
                    MAC = d.MAC,
                    Model = d.Model,
                    RFID = d.RFID,
                    Battery = 1,
                    Firmware = "3.0.0",
                    LastSynced = null,
                    EmployeeId = null,
                    PrimaryAccountId = null,
                    MasterPassword = null,
                    AcceessProfileId = "default",
                    ImportedAt = DateTime.UtcNow,
                    UsePin = true
                })
                .ToList();

                // Add devices if count > 0
                if (toImport.Count > 0)
                {
                    // Save devices
                    await _deviceRepository.AddRangeAsync(toImport);
                    devicesImported = toImport;
                }

                return (devicesExists, devicesImported, message);
            }
            else
            {
                message = "File is recognized, but it is no devices to import. Check file structure and try again.";
                return (devicesExists, devicesImported, message);
            }
        }

        public async Task EditDeviceRfidAsync(Device device)
        {
            if (device == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            if (string.IsNullOrWhiteSpace(device.RFID))
            {
                device.RFID = null;
            }

            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "RFID" });
        }

        public async Task UpdateDevicePropAsync(string deviceId, int batteryCharge, string version)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new Exception($"Device not found, ID: {deviceId}");
            }

            device.Battery = batteryCharge;
            device.Firmware = version;
            device.LastSynced = DateTime.UtcNow;

            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "Battery", "Firmware", "LastSynced" });
        }

        public async Task UpdateProfileAsync(string[] devices, string profileId)
        {
            if (devices == null)
            {
                throw new NullReferenceException(nameof(devices));
            }
            if (profileId == null)
            {
                throw new NullReferenceException(nameof(devices));
            }

            var profile = await _deviceAccessProfilesService.GetByIdAsync(profileId);
            if (profileId == null)
            {
                throw new Exception("Profile not found");
            }

            foreach (var deviceId in devices)
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    throw new Exception($"Device not found, ID: {deviceId}");
                }

                device.AcceessProfileId = profileId;
                await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "AcceessProfileId" });

                // Delete all previous tasks for update profile
                var allProfileTasks = await _deviceTaskRepository
                    .Query()
                    .Where(t => t.DeviceId == deviceId && t.Operation == TaskOperation.Profile)
                    .ToListAsync();
                await _deviceTaskRepository.DeleteRangeAsync(allProfileTasks);

                await _remoteTaskService.AddTaskAsync(new DeviceTask
                {
                    Operation = TaskOperation.Profile,
                    CreatedAt = DateTime.UtcNow,
                    DeviceId = device.Id,
                    Password = device.MasterPassword
                });
            }
            _remoteTaskService.StartTaskProcessing(devices);
        }

        public async Task UnlockPinAsync(string deviceId)
        {
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new Exception($"Device not found, ID: {deviceId}");
            }

            // Update device state
            device.State = DeviceState.PendingUnlock;
            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "State" });

            // Create task
            await _remoteTaskService.AddTaskAsync(new DeviceTask
            {
                DeviceId = device.Id,
                Password = device.MasterPassword,
                Operation = TaskOperation.UnlockPin,
                CreatedAt = DateTime.UtcNow
            });

            // Add event
            await _workstationEventService.AddEventAsync(new WorkstationEvent
            {
                Date = DateTime.UtcNow,
                EventId = WorkstationEventType.DevicePendingUnlock,
                SeverityId = WorkstationEventSeverity.Info,
                DeviceId = deviceId
            });

            _remoteTaskService.StartTaskProcessing(deviceId);
        }

        public async Task<bool> ExistAsync(Expression<Func<Device, bool>> predicate)
        {
            return await _deviceRepository.ExistAsync(predicate);
        }
    }
}