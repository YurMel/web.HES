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
        None,
        On,
        Off,
        Encrypts,
        Decrypts,
        WaitingForActivation
    }

    public class DataProtectionService : IDataProtectionService
    {
        public static ProtectionStatus Status { get; private set; }
        private readonly IAsyncRepository<AppSettings> _dataProtectionRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<DataProtectionService> _logger;
        private IDataProtector _dataProtector;

        public DataProtectionService(IAsyncRepository<AppSettings> dataProtectionRepository,
                                     IAsyncRepository<Device> deviceRepository,
                                     IAsyncRepository<DeviceTask> deviceTaskRepository,
                                     IAsyncRepository<SharedAccount> sharedAccountRepository,
                                     IDataProtectionProvider dataProtectionProvider,
                                     ILogger<DataProtectionService> logger)
        {
            _dataProtectionRepository = dataProtectionRepository;
            _deviceRepository = deviceRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _sharedAccountRepository = sharedAccountRepository;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public async void CheckProtectionStatus()
        {
            var appSettings = await _dataProtectionRepository.Query().FirstOrDefaultAsync();
            if (appSettings?.ProtectedValue != null)
            {
                Status = ProtectionStatus.WaitingForActivation;
            }
            else
            {
                Status = ProtectionStatus.Off;
            }
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

        public string Protect(string text)
        {
            return _dataProtector.Protect(text);
        }

        public string Unprotect(string text)
        {
            return _dataProtector.Unprotect(text);
        }

        private async Task ProtectAllDataAsync()
        {
            try
            {
                var devices = await _deviceRepository.GetAllAsync();
                foreach (var device in devices)
                {
                    if (device.MasterPassword != null)
                    {
                        device.MasterPassword = Protect(device.MasterPassword);
                        await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "MasterPassword" });
                    }
                }

                var deviceTasks = await _deviceTaskRepository.GetAllAsync();
                foreach (var task in deviceTasks)
                {
                    if (task.Password != null)
                    {
                        task.Password = Protect(task.Password);
                        await _deviceTaskRepository.UpdateOnlyPropAsync(task, new string[] { "Password" });
                    }
                }

                var sharedAccounts = await _sharedAccountRepository.GetAllAsync();
                foreach (var account in sharedAccounts)
                {
                    if (account.Password != null)
                    {
                        account.Password = Protect(account.Password);
                        await _sharedAccountRepository.UpdateOnlyPropAsync(account, new string[] { "Password" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        private async Task UnprotectAllDataAsync()
        {
            try
            {
                var devices = await _deviceRepository.GetAllAsync();
                foreach (var device in devices)
                {
                    if (device.MasterPassword != null)
                    {
                        device.MasterPassword = Unprotect(device.MasterPassword);
                        await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "MasterPassword" });
                    }
                }

                var deviceTasks = await _deviceTaskRepository.GetAllAsync();
                foreach (var task in deviceTasks)
                {
                    if (task.Password != null)
                    {
                        task.Password = Unprotect(task.Password);
                        await _deviceTaskRepository.UpdateOnlyPropAsync(task, new string[] { "Password" });
                    }
                }

                var sharedAccounts = await _sharedAccountRepository.GetAllAsync();
                foreach (var account in sharedAccounts)
                {
                    if (account.Password != null)
                    {
                        account.Password = Unprotect(account.Password);
                        await _sharedAccountRepository.UpdateOnlyPropAsync(account, new string[] { "Password" });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }       
    }
}