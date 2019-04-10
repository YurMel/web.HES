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
        Task ImportDevices(IList<Device> devices);
    }
}