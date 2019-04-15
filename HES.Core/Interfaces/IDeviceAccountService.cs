using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceAccountService
    {
        Task<IList<DeviceAccount>> GetAllAsync();
        Task<IList<DeviceAccount>> GetAllWhereAsync(Expression<Func<DeviceAccount, bool>> predicate);
        Task<IList<DeviceAccount>> GetAllIncludeAsync(params Expression<Func<DeviceAccount, object>>[] navigationProperties);
        Task<DeviceAccount> GetFirstOrDefaulAsync();
        Task<DeviceAccount> GetFirstOrDefaulAsync(Expression<Func<DeviceAccount, bool>> match);
        Task<DeviceAccount> GetFirstOrDefaulIncludeAsync(Expression<Func<DeviceAccount, bool>> where, params Expression<Func<DeviceAccount, object>>[] navigationProperties);
        Task<DeviceAccount> GetByIdAsync(dynamic id);
        Task<DeviceAccount> AddAsync(DeviceAccount entity);
        Task<DeviceTask> AddAsync(DeviceTask entity);
        Task<IList<DeviceAccount>> AddRangeAsync(IList<DeviceAccount> entity);
        Task<IList<DeviceTask>> AddRangeAsync(IList<DeviceTask> entity);
        Task UpdateAsync(DeviceAccount entity);
        Task UpdateOnlyPropAsync(DeviceAccount entity, string[] properties);
        Task DeleteAsync(DeviceAccount entity);
        bool Exist(Expression<Func<DeviceAccount, bool>> predicate);
    }
}