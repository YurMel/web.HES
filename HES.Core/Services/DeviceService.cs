using HES.Core.Entities;
using HES.Core.Interfaces;
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

        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAesCryptography _aes;

        public DeviceService(IAsyncRepository<Device> deviceRepository, IAesCryptography aes)
        {
            _deviceRepository = deviceRepository;
            _aes = aes;
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
                var isExist = await _deviceRepository.GetAllWhereAsync(d => objects.Select(o => o.Id).Contains(d.Id));
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
                    Firmware = null,
                    LastSynced = null,
                    EmployeeId = null,
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

        public bool Exist(Expression<Func<Device, bool>> predicate)
        {
            return _deviceRepository.Exist(predicate);
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

        /// <summary>
        /// full battery is 1, discharged 0 
        /// </summary>
        public async Task UpdateBatteryChargeAsync(string deviceId, int batteryCharge)
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
            device.LastSynced = DateTime.UtcNow;

            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "Battery", "LastSynced" });
        }

        public async Task UpdateFirmwareVersionAsync(string deviceId, string version)
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

            device.Firmware = version;
            device.LastSynced = DateTime.UtcNow;

            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "Firmware", "LastSynced" });
        }
    }
}