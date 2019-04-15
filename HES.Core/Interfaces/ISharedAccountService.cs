using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISharedAccountService
    {
        Task<IList<SharedAccount>> GetAllAsync();
        Task<SharedAccount> GetByIdAsync(dynamic id);
        Task<IList<SharedAccount>> GetAllWhereAsync(Expression<Func<SharedAccount, bool>> predicate);
        Task<IList<SharedAccount>> GetAllIncludeAsync(params Expression<Func<SharedAccount, object>>[] navigationProperties);
        Task<SharedAccount> FirstOrDefaulAsync();
        Task<SharedAccount> FirstOrDefaulAsync(Expression<Func<SharedAccount, bool>> match);
        Task<SharedAccount> FirstOrDefaulIncludeAsync(Expression<Func<SharedAccount, bool>> where, params Expression<Func<SharedAccount, object>>[] navigationProperties);
        Task<SharedAccount> AddAsync(SharedAccount entity);
        Task<IList<SharedAccount>> AddRangeAsync(IList<SharedAccount> entity);
        Task UpdateOnlyPropAsync(SharedAccount entity, string[] properties);
        Task DeleteAsync(SharedAccount entity);
        bool Exist(Expression<Func<SharedAccount, bool>> predicate);
        //DeviceAccount CreateDeviceAccount(string Id, string Name, string Urls, string Apps, string Login, AccountType Type, AccountStatus Status, string OtpUpdatedAt, string EmployeeId, string DeviceId, string SharedAccount);
        //DeviceTask CreateDeviceTask(string DeviceId, string DeviceAccountId, string Password, string OtpSecret, DateTime CreatedAt, TaskOperation Operation, bool NameChanged, bool UrlsChanged, bool AppsChanged, bool LoginChanged, bool PasswordChanged, bool OtpSecretChanged);
    }
}