using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
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

        public async Task<IList<SharedAccount>> GetAllAsync()
        {
            return await _sharedAccountRepository.GetAllAsync();
        }

        public async Task<SharedAccount> GetByIdAsync(dynamic id)
        {
            return await _sharedAccountRepository.GetByIdAsync(id);
        }

        public async Task<IList<SharedAccount>> GetAllWhereAsync(Expression<Func<SharedAccount, bool>> predicate)
        {
            return await _sharedAccountRepository.GetAllWhereAsync(predicate);
        }

        public async Task<IList<SharedAccount>> GetAllIncludeAsync(params Expression<Func<SharedAccount, object>>[] navigationProperties)
        {
            return await _sharedAccountRepository.GetAllIncludeAsync(navigationProperties);
        }

        public async Task<SharedAccount> FirstOrDefaulAsync()
        {
            return await _sharedAccountRepository.GetFirstOrDefaulAsync();
        }

        public async Task<SharedAccount> FirstOrDefaulAsync(Expression<Func<SharedAccount, bool>> match)
        {
            return await _sharedAccountRepository.GetFirstOrDefaulAsync(match);
        }

        public async Task<SharedAccount> FirstOrDefaulIncludeAsync(Expression<Func<SharedAccount, bool>> where, params Expression<Func<SharedAccount, object>>[] navigationProperties)
        {
            return await _sharedAccountRepository.GetFirstOrDefaulIncludeAsync(where, navigationProperties);
        }

        public async Task<SharedAccount> AddAsync(SharedAccount entity)
        {
            return await _sharedAccountRepository.AddAsync(entity);
        }

        public async Task<IList<SharedAccount>> AddRangeAsync(IList<SharedAccount> entity)
        {
            return await _sharedAccountRepository.AddRangeAsync(entity);
        }

        public async Task UpdateOnlyPropAsync(SharedAccount entity, string[] properties)
        {
            await _sharedAccountRepository.UpdateOnlyPropAsync(entity, properties);
        }

        public async Task DeleteAsync(SharedAccount entity)
        {
            await _sharedAccountRepository.DeleteAsync(entity);
        }

        public bool Exist(Expression<Func<SharedAccount, bool>> predicate)
        {
            return _sharedAccountRepository.Exist(predicate);
        }

        //public DeviceAccount CreateDeviceAccount(string Id, string Name, string Urls, string Apps, string Login, AccountType Type, AccountStatus Status, string OtpUpdatedAt, string EmployeeId, string DeviceId, string SharedAccount)
        //{
        //    var deviceAccount = new DeviceAccount();
        //    DateTime? date = DateTime.Now;

        //    deviceAccount.Id = Id;
        //    deviceAccount.Name = Name;
        //    deviceAccount.Urls = Urls;
        //    deviceAccount.Apps = Apps;
        //    deviceAccount.Login = Login;
        //    deviceAccount.Type = Type;
        //    deviceAccount.Status = Status;
        //    deviceAccount.LastSyncedAt = null;
        //    deviceAccount.CreatedAt = DateTime.Now;
        //    deviceAccount.PasswordUpdatedAt = DateTime.Now;
        //    deviceAccount.OtpUpdatedAt = OtpUpdatedAt != null ? date : null;
        //    deviceAccount.Deleted = false;
        //    deviceAccount.EmployeeId = EmployeeId;
        //    deviceAccount.DeviceId = DeviceId;
        //    deviceAccount.SharedAccountId = SharedAccount;

        //    return deviceAccount;
        //}

        //public DeviceTask CreateDeviceTask(string DeviceId, string DeviceAccountId, string Password, string OtpSecret, DateTime CreatedAt, TaskOperation Operation, bool NameChanged, bool UrlsChanged, bool AppsChanged, bool LoginChanged, bool PasswordChanged, bool OtpSecretChanged)
        //{
        //    var deviceTask = new DeviceTask();

        //    deviceTask.DeviceId = DeviceId;
        //    deviceTask.DeviceAccountId = DeviceAccountId;
        //    deviceTask.Password = Password;
        //    deviceTask.OtpSecret = OtpSecretChanged == true ? OtpSecret : null;
        //    deviceTask.OtpSecret = OtpSecret;
        //    deviceTask.CreatedAt = CreatedAt;
        //    deviceTask.Operation = Operation;
        //    deviceTask.NameChanged = NameChanged;
        //    deviceTask.UrlsChanged = UrlsChanged;
        //    deviceTask.AppsChanged = AppsChanged;
        //    deviceTask.LoginChanged = LoginChanged;
        //    deviceTask.PasswordChanged = PasswordChanged;
        //    deviceTask.OtpSecretChanged = OtpSecretChanged;

        //    return deviceTask;
        //}
    }
}