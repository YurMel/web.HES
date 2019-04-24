using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class SharedAccountService : ISharedAccountService
    {
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;

        public SharedAccountService(IAsyncRepository<SharedAccount> sharedAccountRepository)
        {
            _sharedAccountRepository = sharedAccountRepository;
        }

        public IQueryable<SharedAccount> SharedAccountQuery()
        {
            return _sharedAccountRepository.Query();
        }

        //public async Task<IList<SharedAccount>> GetAllAsync()
        //{
        //    return await _sharedAccountRepository.GetAllAsync();
        //}

        //public async Task<IList<SharedAccount>> GetAllWhereAsync(Expression<Func<SharedAccount, bool>> predicate)
        //{
        //    return await _sharedAccountRepository.GetAllWhereAsync(predicate);
        //}

        //public async Task<IList<SharedAccount>> GetAllIncludeAsync(params Expression<Func<SharedAccount, object>>[] navigationProperties)
        //{
        //    return await _sharedAccountRepository.GetAllIncludeAsync(navigationProperties);
        //}

        //public async Task<SharedAccount> GetFirstOrDefaulAsync()
        //{
        //    return await _sharedAccountRepository.GetFirstOrDefaulAsync();
        //}

        //public async Task<SharedAccount> GetFirstOrDefaulAsync(Expression<Func<SharedAccount, bool>> match)
        //{
        //    return await _sharedAccountRepository.GetFirstOrDefaulAsync(match);
        //}

        //public async Task<SharedAccount> GetFirstOrDefaulIncludeAsync(Expression<Func<SharedAccount, bool>> where, params Expression<Func<SharedAccount, object>>[] navigationProperties)
        //{
        //    return await _sharedAccountRepository.GetFirstOrDefaulIncludeAsync(where, navigationProperties);
        //}

        public async Task<SharedAccount> SharedAccountGetByIdAsync(dynamic id)
        {
            return await _sharedAccountRepository.GetByIdAsync(id);
        }

        public async Task<SharedAccount> CreateSharedAccountAsync(SharedAccount sharedAccount, InputModel input)
        {
            if (sharedAccount == null || input == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var exist = _sharedAccountRepository.Query().Where(s => s.Name == sharedAccount.Name).Where(s => s.Login == sharedAccount.Login).Where(s => s.Deleted == false).Any();
            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }
            // Set password
            sharedAccount.Password = input.Password;
            // Set password date change
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;
            // Set otp date change
            if (!string.IsNullOrWhiteSpace(sharedAccount.OtpSecret))
                sharedAccount.OtpSecretChangedAt = DateTime.UtcNow;

            return await _sharedAccountRepository.AddAsync(sharedAccount);
        }

        public async Task EditSharedAccountAsync(SharedAccount sharedAccount)
        {
            if (sharedAccount == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var exist = _sharedAccountRepository.Query().Where(s => s.Name == sharedAccount.Name).Where(s => s.Login == sharedAccount.Login).Where(s => s.Deleted == false).Where(s => s.Id != sharedAccount.Id).Any();
            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }
            // Update Shared Account
            string[] properties = { "Name", "Urls", "Apps", "Login" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);
        }

        public async Task EditSharedAccountPwdAsync(SharedAccount sharedAccount, InputModel input)
        {
            if (sharedAccount == null || input == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            // Update Shared Account
            sharedAccount.Password = input.Password;
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;
            string[] properties = { "Password", "PasswordChangedAt" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);
        }

        public async Task EditSharedAccountOtpAsync(SharedAccount sharedAccount)
        {
            if (sharedAccount == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            // Update Shared Account
            sharedAccount.OtpSecretChangedAt = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? new DateTime?(DateTime.UtcNow) : null;
            string[] properties = { "OtpSecret", "OtpSecretChangedAt" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);
        }

        public async Task DeleteSharedAccountAsync(string id)
        {
            if (id == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var sharedAccount = await _sharedAccountRepository.GetByIdAsync(id);
            if (sharedAccount == null)
            {
                throw new Exception("Shared account does not exist.");
            }

            sharedAccount.Deleted = true;
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { "Deleted" });
        }

        public bool Exist(Expression<Func<SharedAccount, bool>> predicate)
        {
            return _sharedAccountRepository.Exist(predicate);
        }
    }
}