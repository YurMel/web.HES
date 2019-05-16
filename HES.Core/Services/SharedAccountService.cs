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
        private readonly IDataProtectionService _dataProtectionService;

        public SharedAccountService(IAsyncRepository<SharedAccount> sharedAccountRepository,
                                    IDataProtectionService dataProtectionService)
        {
            _sharedAccountRepository = sharedAccountRepository;
            _dataProtectionService = dataProtectionService;
        }

        public IQueryable<SharedAccount> SharedAccountQuery()
        {
            return _sharedAccountRepository.Query();
        }

        public async Task<SharedAccount> SharedAccountGetByIdAsync(dynamic id)
        {
            return await _sharedAccountRepository.GetByIdAsync(id);
        }

        public async Task<SharedAccount> CreateSharedAccountAsync(SharedAccount sharedAccount, InputModel input)
        {
            if (!_dataProtectionService.CanUse())
            {
                throw new Exception("Data protection not activated or is busy.");
            }

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
            sharedAccount.Password = _dataProtectionService.Protect(input.Password);
            // Set password date change
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;
            // Set otp date change
            if (!string.IsNullOrWhiteSpace(sharedAccount.OtpSecret))
            {
                sharedAccount.OtpSecret = _dataProtectionService.Protect(sharedAccount.OtpSecret);
                sharedAccount.OtpSecretChangedAt = DateTime.UtcNow;
            }

            return await _sharedAccountRepository.AddAsync(sharedAccount);
        }

        public async Task EditSharedAccountAsync(SharedAccount sharedAccount)
        {
            if (!_dataProtectionService.CanUse())
            {
                throw new Exception("Data protection not activated or is busy.");
            }

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
            if (!_dataProtectionService.CanUse())
            {
                throw new Exception("Data protection not activated or is busy.");
            }

            if (sharedAccount == null || input == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            // Update Shared Account
            sharedAccount.Password = _dataProtectionService.Protect(input.Password);
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;
            string[] properties = { "Password", "PasswordChangedAt" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);
        }

        public async Task EditSharedAccountOtpAsync(SharedAccount sharedAccount)
        {
            if (!_dataProtectionService.CanUse())
            {
                throw new Exception("Data protection not activated or is busy.");
            }

            if (sharedAccount == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            // Update Shared Account
            sharedAccount.OtpSecret = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? _dataProtectionService.Protect(sharedAccount.OtpSecret) : null;
            sharedAccount.OtpSecretChangedAt = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? new DateTime?(DateTime.UtcNow) : null;
            string[] properties = { "OtpSecret", "OtpSecretChangedAt" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);
        }

        public async Task DeleteSharedAccountAsync(string id)
        {
            if (!_dataProtectionService.CanUse())
            {
                throw new Exception("Data protection not activated or is busy.");
            }

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