using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public enum ProtectionStatus
    {
        On,
        Off,
        EncryptOrDecrypt,
        WaitingForActivation
        
    }

    public class DataProtectionService : IDataProtectionService
    {
        public static ProtectionStatus Status { get; }
        private readonly IAsyncRepository<AppSettings> _dataProtectionRepository;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<DataProtectionService> _logger;
        private IDataProtector _dataProtector;

        public DataProtectionService(IAsyncRepository<AppSettings> dataProtectionRepository,
                                     IDataProtectionProvider dataProtectionProvider,
                                     ILogger<DataProtectionService> logger)
        {
            _dataProtectionRepository = dataProtectionRepository;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public async Task ActivateDataProtectionAsync(string password)
        {
            try
            {
                // Temp protector
                var tempProtector = _dataProtectionProvider.CreateProtector(password);
                // Get value
                var appSettings = await _dataProtectionRepository.Query().FirstOrDefaultAsync();
                // If no error occurred during the decrypt, then the password is correct
                var unprotectedValue = tempProtector.Unprotect(appSettings.ProtectedValue);
                // Create protector
                _dataProtector = _dataProtectionProvider.CreateProtector(password);
            }
            catch (CryptographicException)
            {
                _logger.LogWarning("Activate: Invalid password");
                throw new Exception("Invalid password");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task EnableDataProtectionAsync(string password)
        {
            if (password == null)
            {
                throw new Exception("The password must not be null.");
            }

            var appSettings = await _dataProtectionRepository.Query().FirstOrDefaultAsync();
            if (appSettings != null)
            {
                if (appSettings.ProtectedValue != null)
                {
                    throw new Exception("The password already added.");
                }
                // Create protector
                _dataProtector = _dataProtectionProvider.CreateProtector(password);
                // Protect value
                var protectedValue = _dataProtector.Protect(Guid.NewGuid().ToString());
                appSettings.ProtectedValue = protectedValue;

                await _dataProtectionRepository.UpdateOnlyPropAsync(appSettings, new string[] { "ProtectedValue" });
            }
            else
            {
                // Create protector
                _dataProtector = _dataProtectionProvider.CreateProtector(password);
                // Protect value
                var protectedValue = _dataProtector.Protect(Guid.NewGuid().ToString());

                await _dataProtectionRepository.AddAsync(new AppSettings() { ProtectedValue = protectedValue });
            }

            await ProtectAllDataAsync();
        }

        public async Task DisableDataProtectionAsync(string password)
        {
            try
            {
                // Temp protector
                var tempProtector = _dataProtectionProvider.CreateProtector(password);
                // Get value
                var appSettings = await _dataProtectionRepository.Query().FirstOrDefaultAsync();
                // If no error occurred during the decrypt, then the password is correct
                var unprotectedValue = tempProtector.Unprotect(appSettings.ProtectedValue);
                // Unprotect
                await UnprotectAllDataAsync();
                // Disable protector
                _dataProtector = null;
                appSettings.ProtectedValue = null;
                await _dataProtectionRepository.UpdateOnlyPropAsync(appSettings, new string[] { "ProtectedValue" });
            }
            catch (CryptographicException)
            {
                _logger.LogWarning("Disable: Invalid password");
                throw new Exception("Invalid password");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public async Task ChangeDataProtectionPasswordAsync(string oldPassword, string newPassword)
        {
            if (oldPassword == null || newPassword == null)
            {
                throw new Exception("The payload must not be null.");
            }

            try
            {
                // Temp protector
                var tempProtector = _dataProtectionProvider.CreateProtector(oldPassword);
                // Get value
                var appSettings = await _dataProtectionRepository.Query().FirstOrDefaultAsync();
                // If no error occurred during the decrypt, then the password is correct
                var unprotectedValue = tempProtector.Unprotect(appSettings.ProtectedValue);
                // Unprotect all
                await UnprotectAllDataAsync();
                // Create protector
                _dataProtector = _dataProtectionProvider.CreateProtector(newPassword);
                // Protect value
                var protectedValue = _dataProtector.Protect(Guid.NewGuid().ToString());
                appSettings.ProtectedValue = protectedValue;

                await _dataProtectionRepository.UpdateOnlyPropAsync(appSettings, new string[] { "ProtectedValue" });
                // Protect all
                await ProtectAllDataAsync();
            }
            catch (CryptographicException)
            {
                _logger.LogWarning("ChangePassword: Invalid password");
                throw new Exception("Invalid password");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private async Task ProtectAllDataAsync()
        {
            //TODO
            await Task.CompletedTask;
        }

        private async Task UnprotectAllDataAsync()
        {
            //TODO
            await Task.CompletedTask;
        }
    }
}