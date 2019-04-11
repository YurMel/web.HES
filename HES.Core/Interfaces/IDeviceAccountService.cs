using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceAccountService
    {
        Task<DeviceAccount> GetByIdAsync(dynamic id);
        Task<IList<DeviceAccount>> GetWhereAsync(Expression<Func<DeviceAccount, bool>> predicate);
        Task<IList<DeviceAccount>> GetAllIncludeAsync(params Expression<Func<DeviceAccount, object>>[] navigationProperties);
        Task<DeviceAccount> GetFirstOrDefaulAsync(Expression<Func<DeviceAccount, bool>> match);
        Task<DeviceAccount> GetFirstOrDefaulIncludeAsync(Expression<Func<DeviceAccount, bool>> where, params Expression<Func<DeviceAccount, object>>[] navigationProperties);
        Task<DeviceAccount> AddAsync(DeviceAccount entity);
        Task<DeviceTask> AddAsync(DeviceTask entity);
        Task<IList<DeviceAccount>> AddRangeAsync(IList<DeviceAccount> entity);
        Task<IList<DeviceTask>> AddRangeAsync(IList<DeviceTask> entity);
        Task UpdateOnlyPropAsync(DeviceAccount entity, string[] properties);
        Task DeleteAsync(DeviceAccount entity);
        bool Exist(Expression<Func<DeviceAccount, bool>> predicate);
        DeviceAccount CreateDeviceAccount(string Id, string Name, string Urls, string Apps, string Login, AccountType Type, AccountStatus Status, string OtpUpdatedAt, string EmployeeId, string DeviceId, string SharedAccount);
        DeviceTask CreateDeviceTask(string DeviceId, string DeviceAccountId, string Password, string OtpSecret, DateTime CreatedAt, TaskOperation Operation, bool NameChanged, bool UrlsChanged, bool AppsChanged, bool LoginChanged, bool PasswordChanged, bool OtpSecretChanged);
    }
}