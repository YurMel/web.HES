using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IRemoteTaskService _remoteTaskService;


        public EmployeeService(IAsyncRepository<Employee> employeeRepository,
                               IAsyncRepository<Device> deviceRepository,
                               IAsyncRepository<DeviceAccount> deviceAccountRepository,
                               IAsyncRepository<DeviceTask> deviceTaskRepository,
                               IAsyncRepository<SharedAccount> sharedAccountRepository,
                               IRemoteTaskService remoteTaskService)
        {
            _employeeRepository = employeeRepository;
            _deviceRepository = deviceRepository;
            _deviceAccountRepository = deviceAccountRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _sharedAccountRepository = sharedAccountRepository;
            _remoteTaskService = remoteTaskService;
        }

        public async Task<IList<Employee>> GetAllAsync()
        {
            return await _employeeRepository.GetAllAsync();
        }

        public async Task<IList<Employee>> GetAllWhereAsync(Expression<Func<Employee, bool>> predicate)
        {
            return await _employeeRepository.GetAllWhereAsync(predicate);
        }

        public async Task<IList<Employee>> GetAllIncludeAsync(params Expression<Func<Employee, object>>[] navigationProperties)
        {
            return await _employeeRepository.GetAllIncludeAsync(navigationProperties);
        }

        public async Task<Employee> GetFirstOrDefaulAsync()
        {
            return await _employeeRepository.GetFirstOrDefaulAsync();
        }

        public async Task<Employee> GetFirstOrDefaulAsync(Expression<Func<Employee, bool>> match)
        {
            return await _employeeRepository.GetFirstOrDefaulAsync(match);
        }

        public async Task<Employee> GetFirstOrDefaulIncludeAsync(Expression<Func<Employee, bool>> where, params Expression<Func<Employee, object>>[] navigationProperties)
        {
            return await _employeeRepository.GetFirstOrDefaulIncludeAsync(where, navigationProperties);
        }

        public async Task<Employee> GetByIdAsync(dynamic id)
        {
            return await _employeeRepository.GetByIdAsync(id);
        }

        public async Task CreateEmployeeAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            await _employeeRepository.AddAsync(employee);
        }

        public async Task EditEmployeeAsync(Employee employee)
        {
            await _employeeRepository.UpdateAsync(employee);
        }

        public async Task DeleteEmployeeAsync(string id)
        {
            if (id == null)
            {
                throw new Exception("The parameter must not be null.");

            }
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                throw new Exception("Employee does not exist.");
            }
            await _employeeRepository.DeleteAsync(employee);
        }

        public bool Exist(Expression<Func<Employee, bool>> predicate)
        {
            return _employeeRepository.Exist(predicate);
        }

        public async Task AddDeviceAsync(string employeeId, string[] selectedDevices)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
            {
                throw new Exception("Employee does not exist.");
            }

            foreach (var deviceId in selectedDevices)
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                {
                    throw new Exception($"Device does not exist, ID: {deviceId}.");
                }
                if (device.EmployeeId != null)
                {
                    throw new Exception($"Current device already linked to another employee");
                }
                device.EmployeeId = employeeId;
                await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "EmployeeId" });
            }
        }

        public async Task RemoveDeviceAsync(string employeeId, string deviceId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new Exception($"Device does not exist, ID: {deviceId}.");
            }
            if (device.EmployeeId != employeeId)
            {
                throw new Exception("Current device is not linked to employee.");
            }
            device.EmployeeId = null;
            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "EmployeeId" });
        }

        public async Task CreatePersonalAccountAsync(DeviceAccount deviceAccount, InputModel input, string[] selectedDevices)
        {
            if (deviceAccount == null || input == null || selectedDevices == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var exist = _deviceAccountRepository.Query().Where(s => s.Name == deviceAccount.Name).Where(s => s.Login == deviceAccount.Login).Where(s => s.Deleted == false).Any();
            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }

            List<DeviceAccount> Accounts = new List<DeviceAccount>();
            List<DeviceTask> Tasks = new List<DeviceTask>();

            foreach (var deviceId in selectedDevices)
            {
                // Device Account id
                var deviceAccountId = Guid.NewGuid().ToString();
                // Create Device Account
                Accounts.Add(new DeviceAccount { Id = deviceAccountId, Name = deviceAccount.Name, Urls = deviceAccount.Urls, Apps = deviceAccount.Apps, Login = deviceAccount.Login, Type = AccountType.Personal, Status = AccountStatus.Creating, CreatedAt = DateTime.UtcNow, PasswordUpdatedAt = DateTime.UtcNow, OtpUpdatedAt = input.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null, EmployeeId = deviceAccount.EmployeeId, DeviceId = deviceId, SharedAccountId = null });
                // Create Device Task
                //Tasks.Add(new DeviceTask { DeviceAccountId = deviceAccountId, Password = input.Password, OtpSecret = input.OtpSecret, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Create, NameChanged = true, UrlsChanged = true, AppsChanged = true, LoginChanged = true, PasswordChanged = true, OtpSecretChanged = true });
                Tasks.Add(new DeviceTask { DeviceAccountId = deviceAccountId, Name = deviceAccount.Name, Urls = deviceAccount.Urls, Apps = deviceAccount.Apps, Login = deviceAccount.Login, Password = input.Password, OtpSecret = input.OtpSecret, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Create });
            }

            await _deviceAccountRepository.AddRangeAsync(Accounts);

            try
            {
                await _remoteTaskService.AddRangeTaskAsync(Tasks);
            }
            catch (Exception ex)
            {
                await _deviceAccountRepository.DeleteRangeAsync(Accounts);
                throw new Exception(ex.Message);
            }
        }

        public async Task EditPersonalAccountAsync(DeviceAccount deviceAccount)
        {
            if (deviceAccount == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var exist = _deviceAccountRepository.Query().Where(s => s.Name == deviceAccount.Name).Where(s => s.Login == deviceAccount.Login).Where(s => s.Deleted == false).Where(s =>s.Id != deviceAccount.Id).Any();
            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }
            // Update Device Account
            //deviceAccount.UpdatedAt = DateTime.UtcNow;
            deviceAccount.Status = AccountStatus.Updating;
            //string[] properties = { "Name", "Urls", "Apps", "Login", "Status", "UpdatedAt" };
            string[] properties = { "Status" };
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
            // Create Device Task
            //await _deviceTaskRepository.AddAsync(new DeviceTask { DeviceAccountId = deviceAccount.Id, Password = null, OtpSecret = null, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Update, NameChanged = true, UrlsChanged = true, AppsChanged = true, LoginChanged = true, PasswordChanged = false, OtpSecretChanged = false });
            try
            {
                await _remoteTaskService.AddTaskAsync(new DeviceTask { DeviceAccountId = deviceAccount.Id, Name = deviceAccount.Name, Urls = deviceAccount.Urls, Apps = deviceAccount.Apps, Login = deviceAccount.Login, Password = null, OtpSecret = null, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Update });
            }
            catch (Exception ex)
            {
                deviceAccount.Status = AccountStatus.Done;
                await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
                throw new Exception(ex.Message);
            }
        }

        public async Task EditPersonalAccountPwdAsync(DeviceAccount deviceAccount, InputModel input)
        {
            if (deviceAccount == null || input == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            // Update Device Account
            //deviceAccount.UpdatedAt = DateTime.UtcNow;
            //deviceAccount.PasswordUpdatedAt = DateTime.UtcNow;
            deviceAccount.Status = AccountStatus.Updating;
            //string[] properties = { "Status", "UpdatedAt", "PasswordUpdatedAt" };
            string[] properties = { "Status" };
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
            // Create Device Task
            //await _deviceTaskRepository.AddAsync(new DeviceTask { DeviceAccountId = deviceAccount.Id, Password = input.Password, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Update, PasswordChanged = true });
            try
            {
                await _remoteTaskService.AddTaskAsync(new DeviceTask { DeviceAccountId = deviceAccount.Id, Password = input.Password, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Update });
            }
            catch (Exception ex)
            {
                deviceAccount.Status = AccountStatus.Done;
                await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
                throw new Exception(ex.Message);
            }
        }

        public async Task EditPersonalAccountOtpAsync(DeviceAccount deviceAccount, InputModel input)
        {
            if (deviceAccount == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            if (string.IsNullOrWhiteSpace(input.OtpSecret))
            {
                input.OtpSecret = null;
            }

            // Update Device Account
            //deviceAccount.UpdatedAt = DateTime.UtcNow;
            //deviceAccount.OtpUpdatedAt = input.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null;
            deviceAccount.Status = AccountStatus.Updating;
            //string[] properties = { "Status", "UpdatedAt", "OtpUpdatedAt" };
            string[] properties = { "Status" };
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
            // Create Device Task
            //await _deviceTaskRepository.AddAsync(new DeviceTask { DeviceAccountId = deviceAccount.Id, OtpSecret = input.OtpSecret, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Update, OtpSecretChanged = true });
            try
            {
                await _remoteTaskService.AddTaskAsync(new DeviceTask { DeviceAccountId = deviceAccount.Id, OtpSecret = input.OtpSecret, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Update });
            }
            catch (Exception ex)
            {
                deviceAccount.Status = AccountStatus.Done;
                await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
                throw new Exception(ex.Message);
            }
        }

        public async Task AddSharedAccount(string employeeId, string sharedAccountId, string[] selectedDevices)
        {
            if (employeeId == null || sharedAccountId == null || selectedDevices == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            List<DeviceAccount> Accounts = new List<DeviceAccount>();
            List<DeviceTask> Tasks = new List<DeviceTask>();

            foreach (var deviceId in selectedDevices)
            {
                // Get Shared Account
                var sharedAccount = await _sharedAccountRepository.GetByIdAsync(sharedAccountId);
                if (sharedAccount == null)
                {
                    throw new Exception("SharedAccount does not exist.");
                }
                // Create Device Account
                var deviceAccountId = Guid.NewGuid().ToString();
                Accounts.Add(new DeviceAccount { Id = deviceAccountId, Name = sharedAccount.Name, Urls = sharedAccount.Urls, Apps = sharedAccount.Apps, Login = sharedAccount.Login, Type = AccountType.Shared, Status = AccountStatus.Creating, CreatedAt = DateTime.UtcNow, PasswordUpdatedAt = DateTime.UtcNow, OtpUpdatedAt = sharedAccount.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null, EmployeeId = employeeId, DeviceId = deviceId, SharedAccountId = sharedAccountId });
                // Create Device Task
                //Tasks.Add(new DeviceTask { DeviceAccountId = deviceAccountId, Password = sharedAccount.Password, OtpSecret = sharedAccount.OtpSecret, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Create, NameChanged = true, UrlsChanged = true, AppsChanged = true, LoginChanged = true, PasswordChanged = true, OtpSecretChanged = true });
                Tasks.Add(new DeviceTask { DeviceAccountId = deviceAccountId, Name = sharedAccount.Name, Urls = sharedAccount.Urls, Apps = sharedAccount.Apps, Login = sharedAccount.Login, Password = sharedAccount.Password, OtpSecret = sharedAccount.OtpSecret, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Create });
            }

            await _deviceAccountRepository.AddRangeAsync(Accounts);
            //await _deviceTaskRepository.AddRangeAsync(Tasks);

            try
            {
                await _remoteTaskService.AddRangeTaskAsync(Tasks);
            }
            catch (Exception ex)
            {
                await _deviceAccountRepository.DeleteRangeAsync(Accounts);
                throw new Exception(ex.Message);
            }
        }

        public async Task DeleteAccount(string accountId)
        {
            if (accountId == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var deviceAccount = await _deviceAccountRepository.GetByIdAsync(accountId);
            if (deviceAccount == null)
            {
                throw new Exception("Device account does not exist.");
            }
            // Update Device Account
            deviceAccount.Status = AccountStatus.Removing;
            //deviceAccount.UpdatedAt = DateTime.UtcNow;
            //string[] properties = { "Status", "UpdatedAt" };
            string[] properties = { "Status" };
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
            // Create Device Task
            //await _deviceTaskRepository.AddAsync(new DeviceTask { DeviceAccountId = accountId, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Delete });
            try
            {
                // Create Device Task
                await _remoteTaskService.AddTaskAsync(new DeviceTask { DeviceAccountId = accountId, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Delete });
            }
            catch (Exception ex)
            {
                deviceAccount.Status = AccountStatus.Done;
                await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
                throw new Exception(ex.Message);
            }
        }
    }
}