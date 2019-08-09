using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceService
    {
        IQueryable<Device> DeviceQuery();
        Task<Device> DeviceGetByIdAsync(dynamic id);
        Task<(IList<Device> devicesExists, IList<Device> devicesImported, string message)> ImportDevices(string key, byte[] fileContent);
        Task EditDeviceRfidAsync(Device device);
        Task UpdateDevicePropAsync(string deviceId, int batteryCharge, string version);
        Task UpdateProfileAsync(string[] devices, string profileId);
        Task<bool> ExistAsync(Expression<Func<Device, bool>> predicate);
    }
}