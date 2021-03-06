﻿using HES.Core.Entities;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationProximityDeviceService
    {
        IQueryable<WorkstationProximityDevice> Query();
        Task AddProximityDeviceAsync(string workstationId, string[] devicesId);
        Task AddMultipleProximityDevicesAsync(string[] workstationsId, string[] devicesId);
        Task EditProximityDeviceAsync(WorkstationProximityDevice proximityDevice);
        Task DeleteProximityDeviceAsync(string proximityDeviceId);
        Task<IReadOnlyList<DeviceProximitySettingsDto>> GetProximitySettingsAsync(string workstationId);
        Task UpdateProximitySettingsAsync(string workstationId);
    }
}