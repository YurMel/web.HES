using HES.Core.Entities;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationProximityDeviceService : IWorkstationProximityDeviceService
    {
        private readonly IAsyncRepository<WorkstationProximityDevice> _workstationProximityDeviceRepository;
        private readonly IWorkstationService _workstationService;

        public WorkstationProximityDeviceService(IAsyncRepository<WorkstationProximityDevice> workstationProximityDeviceRepository,
                                                 IWorkstationService workstationService)
        {
            _workstationProximityDeviceRepository = workstationProximityDeviceRepository;
            _workstationService = workstationService;
        }

        public IQueryable<WorkstationProximityDevice> Query()
        {
            return _workstationProximityDeviceRepository.Query();
        }

        public async Task AddProximityDeviceAsync(string workstationId, string[] devicesId)
        {
            if (workstationId == null)
            {
                throw new ArgumentNullException(nameof(workstationId));
            }
            if (devicesId == null)
            {
                throw new ArgumentNullException(nameof(devicesId));
            }

            List<WorkstationProximityDevice> proximityDevices = new List<WorkstationProximityDevice>();

            foreach (var deviceId in devicesId)
            {
                var exists = await _workstationProximityDeviceRepository
                .Query()
                .Where(d => d.DeviceId == deviceId)
                .Where(d => d.WorkstationId == workstationId)
                .FirstOrDefaultAsync();

                if (exists != null)
                {
                    throw new Exception($"Device {deviceId} already exist.");
                }

                proximityDevices.Add(new WorkstationProximityDevice
                {
                    WorkstationId = workstationId,
                    DeviceId = deviceId,
                    LockProximity = 40,
                    UnlockProximity = 75,
                    LockTimeout = 3
                });
            }

            await _workstationProximityDeviceRepository.AddRangeAsync(proximityDevices);
            await UpdateProximitySettingsAsync(workstationId);
        }

        public async Task AddMultipleProximityDevicesAsync(string[] workstationsId, string[] devicesId)
        {
            if (workstationsId == null)
            {
                throw new ArgumentNullException(nameof(workstationsId));
            }
            if (devicesId == null)
            {
                throw new ArgumentNullException(nameof(devicesId));
            }

            foreach (var workstation in workstationsId)
            {
                await AddProximityDeviceAsync(workstation, devicesId);
            }
        }

        public async Task EditProximityDeviceAsync(WorkstationProximityDevice proximityDevice)
        {
            if (proximityDevice == null)
                throw new ArgumentNullException(nameof(proximityDevice));

            string[] properties = { "LockProximity", "UnlockProximity", "LockTimeout" };
            await _workstationProximityDeviceRepository.UpdateOnlyPropAsync(proximityDevice, properties);
            await UpdateProximitySettingsAsync(proximityDevice.WorkstationId);
        }

        public async Task DeleteProximityDeviceAsync(string proximityDeviceId)
        {
            if (proximityDeviceId == null)
            {
                throw new ArgumentNullException(nameof(proximityDeviceId));
            }

            var proximityDevice = await _workstationProximityDeviceRepository.GetByIdAsync(proximityDeviceId);
            if (proximityDevice == null)
            {
                throw new Exception("Binding not found.");
            }

            await _workstationProximityDeviceRepository.DeleteAsync(proximityDevice);
            await UpdateProximitySettingsAsync(proximityDevice.WorkstationId);
        }

        public async Task DeleteRangeProximityDevicesAsync(List<WorkstationProximityDevice> proximityDevices)
        {
            if (proximityDevices == null)
            {
                throw new ArgumentNullException(nameof(proximityDevices));
            }

            await _workstationProximityDeviceRepository.DeleteRangeAsync(proximityDevices);

            foreach (var item in proximityDevices)
            {
                await UpdateProximitySettingsAsync(item.WorkstationId);
            }
        }

        public async Task<IReadOnlyList<DeviceProximitySettingsDto>> GetProximitySettingsAsync(string workstationId)
        {
            var workstation = await _workstationService
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workstationId);

            if (workstation == null)
            {
                throw new Exception("Workstation not found");
            }

            var deviceProximitySettings = new List<DeviceProximitySettingsDto>();

            var proximityDevices = await _workstationProximityDeviceRepository
                .Query()
                .Include(i => i.Device)
                .Where(b => b.WorkstationId == workstationId)
                .AsNoTracking()
                .ToListAsync();

            if (workstation.Approved)
            {
                foreach (var proximity in proximityDevices)
                {
                    deviceProximitySettings.Add(new DeviceProximitySettingsDto()
                    {
                        SerialNo = proximity.DeviceId,
                        Mac = proximity.Device.MAC,
                        LockProximity = proximity.LockProximity,
                        UnlockProximity = proximity.UnlockProximity,
                        LockTimeout = proximity.LockTimeout,
                    });
                }
            }

            return deviceProximitySettings;
        }

        public async Task UpdateProximitySettingsAsync(string workstationId)
        {
            var deviceProximitySettings = await GetProximitySettingsAsync(workstationId);

            await AppHub.UpdateProximitySettings(workstationId, deviceProximitySettings);
        }

        public async Task RemoveAllProximityAsync(string deviceId)
        {
            var allProximity = await _workstationProximityDeviceRepository
             .Query()
             .Where(w => w.DeviceId == deviceId)
             .ToListAsync();

            await _workstationProximityDeviceRepository.DeleteRangeAsync(allProximity);

            foreach (var item in allProximity)
            {
                await UpdateProximitySettingsAsync(item.WorkstationId);
            }
        }
    }
}