﻿using HES.Core.Entities;
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
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAesCryptography _aes;

        public DeviceService(IAsyncRepository<Device> deviceRepository, IAesCryptography aes)
        {
            _deviceRepository = deviceRepository;
            _aes = aes;
        }

        public async Task<IList<Device>> GetAllAsync()
        {
            return await _deviceRepository.GetAllIncludeAsync(d => d.Employee);
        }

        public async Task<IList<Device>> GetAllWhereAsync(Expression<Func<Device, bool>> predicate)
        {
            return await _deviceRepository.GetAllWhereAsync(predicate);
        }

        public async Task<IList<Device>> GetAllIncludeAsync(params Expression<Func<Device, object>>[] navigationProperties)
        {
            return await _deviceRepository.GetAllIncludeAsync(navigationProperties);
        }

        public async Task<Device> GetFirstOrDefaulAsync()
        {
            return await _deviceRepository.GetFirstOrDefaulAsync();
        }

        public async Task<Device> GetFirstOrDefaulAsync(Expression<Func<Device, bool>> match)
        {
            return await _deviceRepository.GetFirstOrDefaulAsync(match);
        }

        public async Task<Device> GetFirstOrDefaulIncludeAsync(Expression<Func<Device, bool>> where, params Expression<Func<Device, object>>[] navigationProperties)
        {
            return await _deviceRepository.GetFirstOrDefaulIncludeAsync(where, navigationProperties);
        }

        public async Task<Device> GetByIdAsync(dynamic id)
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
                    RFID = null,
                    Battery = 0,
                    Firmware = null,
                    LastSynced = null,
                    EmployeeId = null,
                    ImportedAt = DateTime.UtcNow,
                    DeviceKey = d.DeviceKey,
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

        class MyHideezDevice
        {
            public string Id { get; set; }
            public string MAC { get; set; }
            public string ManufacturerUserId { get; set; }
            public string Model { get; set; }
            public string BootLoaderVersion { get; set; }
            public DateTime Manufactured { get; set; }
            public string CpuSerialNo { get; set; }
            public byte[] DeviceKey { get; set; }
            public int? BleDeviceBatchId { get; set; }
            public string RegisteredUserId { get; set; }
        }
    }
}