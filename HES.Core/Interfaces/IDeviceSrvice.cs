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
        //Task<IList<Device>> GetAllAsync();
        //Task<IList<Device>> GetAllWhereAsync(Expression<Func<Device, bool>> predicate);
        //Task<IList<Device>> GetAllIncludeAsync(params Expression<Func<Device, object>>[] navigationProperties);
        //Task<Device> GetFirstOrDefaulAsync();
        //Task<Device> GetFirstOrDefaulAsync(Expression<Func<Device, bool>> match);
        //Task<Device> GetFirstOrDefaulIncludeAsync(Expression<Func<Device, bool>> where, params Expression<Func<Device, object>>[] navigationProperties);
        IQueryable<Device> DeviceQuery();
        Task<Device> DeviceGetByIdAsync(dynamic id);
        Task<(IList<Device> devicesExists, IList<Device> devicesImported, string message)> ImportDevices(string key, byte[] fileContent);
        bool Exist(Expression<Func<Device, bool>> predicate);
    }
}