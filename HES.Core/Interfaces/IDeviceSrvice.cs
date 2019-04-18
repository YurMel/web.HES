using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceService
    {
        Task<IList<Device>> GetAllAsync();
        Task<IList<Device>> GetAllWhereAsync(Expression<Func<Device, bool>> predicate);
        Task<IList<Device>> GetAllIncludeAsync(params Expression<Func<Device, object>>[] navigationProperties);
        Task<Device> GetFirstOrDefaulAsync();
        Task<Device> GetFirstOrDefaulAsync(Expression<Func<Device, bool>> match);
        Task<Device> GetFirstOrDefaulIncludeAsync(Expression<Func<Device, bool>> where, params Expression<Func<Device, object>>[] navigationProperties);
        Task<Device> GetByIdAsync(dynamic id);
        Task<Device> AddAsync(Device entity);
        Task<IList<Device>> AddRangeAsync(IList<Device> entity);
        Task UpdateAsync(Device entity);
        Task UpdateOnlyPropAsync(Device entity, string[] properties);
        Task DeleteAsync(Device entity);
        bool Exist(Expression<Func<Device, bool>> predicate);
        Task ImportDevices(IList<Device> devices);
    }
}