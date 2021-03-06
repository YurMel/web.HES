﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class SharedAccountService : ISharedAccountService
    {
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        private readonly IRemoteTaskService _remoteTaskService;
        private readonly IDataProtectionService _dataProtectionService;

        public SharedAccountService(IAsyncRepository<SharedAccount> sharedAccountRepository,
                                    IAsyncRepository<DeviceAccount> deviceAccountRepository,
                                    IRemoteTaskService remoteTaskService,
                                    IDataProtectionService dataProtectionService)
        {
            _sharedAccountRepository = sharedAccountRepository;
            _deviceAccountRepository = deviceAccountRepository;
            _remoteTaskService = remoteTaskService;
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
            _dataProtectionService.Validate();

            if (sharedAccount == null || input == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            var exist = await _sharedAccountRepository
                .Query()
                .Where(s => s.Name == sharedAccount.Name)
                .Where(s => s.Login == sharedAccount.Login)
                .Where(s => s.Deleted == false)
                .AnyAsync();

            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }

            // Validate url
            if (sharedAccount.Urls != null)
            {
                List<string> verifiedUrls = new List<string>();
                foreach (var url in sharedAccount.Urls.Split(";"))
                {
                    string uriString = url;
                    string domain = string.Empty;

                    if (string.IsNullOrWhiteSpace(uriString))
                    {
                        throw new Exception("Not correct url");
                    }

                    if (!uriString.Contains(Uri.SchemeDelimiter))
                    {
                        uriString = string.Concat(Uri.UriSchemeHttp, Uri.SchemeDelimiter, uriString);
                    }

                    domain = new Uri(uriString).Host;

                    if (domain.StartsWith("www."))
                        domain = domain.Remove(0, 4);

                    verifiedUrls.Add(domain);
                }
                sharedAccount.Urls = string.Join(";", verifiedUrls.ToArray());
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
            _dataProtectionService.Validate();

            if (sharedAccount == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            var exist = await _sharedAccountRepository
                .Query()
                .Where(s => s.Name == sharedAccount.Name)
                .Where(s => s.Login == sharedAccount.Login)
                .Where(s => s.Deleted == false)
                .Where(s => s.Id != sharedAccount.Id)
                .AnyAsync();

            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }

            // Validate url
            if (sharedAccount.Urls != null)
            {
                List<string> verifiedUrls = new List<string>();
                foreach (var url in sharedAccount.Urls.Split(";"))
                {
                    string uriString = url;
                    string domain = string.Empty;

                    if (string.IsNullOrWhiteSpace(uriString))
                    {
                        throw new Exception("Not correct url");
                    }

                    if (!uriString.Contains(Uri.SchemeDelimiter))
                    {
                        uriString = string.Concat(Uri.UriSchemeHttp, Uri.SchemeDelimiter, uriString);
                    }

                    domain = new Uri(uriString).Host;

                    if (domain.StartsWith("www."))
                        domain = domain.Remove(0, 4);

                    verifiedUrls.Add(domain);
                }
                sharedAccount.Urls = string.Join(";", verifiedUrls.ToArray());
            }

            // Update Shared Account
            string[] properties = { "Name", "Urls", "Apps", "Login" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);

            // Update all device accounts
            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountRepository
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    Name = sharedAccount.Name,
                    Urls = sharedAccount.Urls ?? string.Empty,
                    Apps = sharedAccount.Apps ?? string.Empty,
                    Login = sharedAccount.Login,
                    Password = null,
                    OtpSecret = null,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            // Update device accounts
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt" });

            // Create Tasks
            await _remoteTaskService.AddRangeAsync(tasks);

            // Start task processing
            var devices = deviceAccounts.Select(s => s.DeviceId).ToList();
            _remoteTaskService.StartTaskProcessing(devices);
        }

        public async Task EditSharedAccountPwdAsync(SharedAccount sharedAccount, InputModel input)
        {
            _dataProtectionService.Validate();

            if (sharedAccount == null || input == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            // Update Shared Account
            sharedAccount.Password = _dataProtectionService.Protect(input.Password);
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;
            string[] properties = { "Password", "PasswordChangedAt" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);

            // Update all device accounts
            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountRepository
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    Password = sharedAccount.Password,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            // Update device accounts
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt" });

            // Create Tasks
            await _remoteTaskService.AddRangeAsync(tasks);

            // Start task processing
            var devices = deviceAccounts.Select(s => s.DeviceId).ToList();
            _remoteTaskService.StartTaskProcessing(devices);
        }

        public async Task EditSharedAccountOtpAsync(SharedAccount sharedAccount, InputModel input)
        {
            _dataProtectionService.Validate();

            if (sharedAccount == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            // Update Shared Account
            sharedAccount.OtpSecret = !string.IsNullOrWhiteSpace(input.OtpSecret) ? _dataProtectionService.Protect(input.OtpSecret) : null;
            sharedAccount.OtpSecretChangedAt = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? new DateTime?(DateTime.UtcNow) : null;
            string[] properties = { "OtpSecret", "OtpSecretChangedAt" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);

            // Update all device accounts
            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountRepository
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    OtpSecret = sharedAccount.OtpSecret ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            // Update device accounts
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt" });

            // Create Tasks
            await _remoteTaskService.AddRangeAsync(tasks);

            // Start task processing
            var devices = deviceAccounts.Select(s => s.DeviceId).ToList();
            _remoteTaskService.StartTaskProcessing(devices);
        }

        public async Task DeleteSharedAccountAsync(string id)
        {
            _dataProtectionService.Validate();

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

            // Update all device accounts
            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountRepository
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Removing;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,         
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Delete,
                    DeviceId = deviceAccount.DeviceId  
                });
            }

            // Update device accounts
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt" });

            // Create Tasks
            await _remoteTaskService.AddRangeAsync(tasks);

            // Start task processing
            var devices = deviceAccounts.Select(s => s.DeviceId).ToList();
            _remoteTaskService.StartTaskProcessing(devices);
        }

        public async Task<bool> ExistAync(Expression<Func<SharedAccount, bool>> predicate)
        {
            return await _sharedAccountRepository.ExistAsync(predicate);
        }
    }
}