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
        bool Exist(Expression<Func<Device, bool>> predicate);
        Task EditDeviceRfidAsync(Device device);
    }
}