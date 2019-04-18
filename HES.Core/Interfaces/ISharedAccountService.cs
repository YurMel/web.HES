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
        Task<IList<SharedAccount>> GetAllWhereAsync(Expression<Func<SharedAccount, bool>> predicate);
        Task<IList<SharedAccount>> GetAllIncludeAsync(params Expression<Func<SharedAccount, object>>[] navigationProperties);
        Task<SharedAccount> GetFirstOrDefaulAsync();
        Task<SharedAccount> GetFirstOrDefaulAsync(Expression<Func<SharedAccount, bool>> match);
        Task<SharedAccount> GetFirstOrDefaulIncludeAsync(Expression<Func<SharedAccount, bool>> where, params Expression<Func<SharedAccount, object>>[] navigationProperties);
        Task<SharedAccount> GetByIdAsync(dynamic id);
        Task<SharedAccount> CreateSharedAccountAsync(SharedAccount sharedAccount, InputModel input);
        Task EditSharedAccountAsync(SharedAccount sharedAccount);
        Task EditSharedAccountPwdAsync(SharedAccount sharedAccount, InputModel input);
        Task EditSharedAccountOtpAsync(SharedAccount sharedAccount);
        Task DeleteSharedAccountAsync(string id);
        //Task<SharedAccount> AddAsync(SharedAccount entity);
        //Task<IList<SharedAccount>> AddRangeAsync(IList<SharedAccount> entity);
        //Task UpdateAsync(SharedAccount entity);
        //Task UpdateOnlyPropAsync(SharedAccount entity, string[] properties);
        //Task DeleteAsync(SharedAccount entity);
        bool Exist(Expression<Func<SharedAccount, bool>> predicate);
    }
}