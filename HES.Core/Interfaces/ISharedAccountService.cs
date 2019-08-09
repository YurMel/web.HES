using HES.Core.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISharedAccountService
    {
        IQueryable<SharedAccount> SharedAccountQuery();
        Task<SharedAccount> SharedAccountGetByIdAsync(dynamic id);
        Task<SharedAccount> CreateSharedAccountAsync(SharedAccount sharedAccount, InputModel input);
        Task EditSharedAccountAsync(SharedAccount sharedAccount);
        Task EditSharedAccountPwdAsync(SharedAccount sharedAccount, InputModel input);
        Task EditSharedAccountOtpAsync(SharedAccount sharedAccount, InputModel input);
        Task DeleteSharedAccountAsync(string id);
        Task<bool> ExistAync(Expression<Func<SharedAccount, bool>> predicate);
    }
}