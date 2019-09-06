using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Entities.Models;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.Security;
using Hideez.SDK.Communication.Utils;
using Microsoft.EntityFrameworkCore;

namespace HES.Core.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IAsyncRepository<Template> _templateRepository;
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IAsyncRepository<Department> _departmentRepository;
        private readonly IAsyncRepository<Position> _positionRepository;
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<WorkstationSession> _workstationSessionRepository;
        private readonly IRemoteTaskService _remoteTaskService;
        private readonly IDataProtectionService _dataProtectionService;

        public EmployeeService(IAsyncRepository<Employee> employeeRepository,
                               IAsyncRepository<Device> deviceRepository,
                               IAsyncRepository<DeviceAccount> deviceAccountRepository,
                               IAsyncRepository<DeviceTask> deviceTaskRepository,
                               IAsyncRepository<SharedAccount> sharedAccountRepository,
                               IAsyncRepository<Template> templateRepository,
                               IAsyncRepository<Company> companyRepository,
                               IAsyncRepository<Department> departmentRepository,
                               IAsyncRepository<Position> positionRepository,
                               IAsyncRepository<WorkstationEvent> workstationEventRepository,
                               IAsyncRepository<WorkstationSession> workstationSessionRepository,
                               IRemoteTaskService remoteTaskService,
                               IDataProtectionService dataProtectionService)
        {
            _employeeRepository = employeeRepository;
            _deviceRepository = deviceRepository;
            _deviceAccountRepository = deviceAccountRepository;
            _deviceTaskRepository = deviceTaskRepository;
            _sharedAccountRepository = sharedAccountRepository;
            _templateRepository = templateRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;
            _workstationEventRepository = workstationEventRepository;
            _workstationSessionRepository = workstationSessionRepository;
            _remoteTaskService = remoteTaskService;
            _dataProtectionService = dataProtectionService;
        }

        public IQueryable<Employee> EmployeeQuery()
        {
            return _employeeRepository.Query();
        }

        public IQueryable<Device> DeviceQuery()
        {
            return _deviceRepository.Query();
        }

        public IQueryable<DeviceAccount> DeviceAccountQuery()
        {
            return _deviceAccountRepository.Query();
        }

        public IQueryable<DeviceTask> DeviceTaskQuery()
        {
            return _deviceTaskRepository.Query();
        }

        public IQueryable<SharedAccount> SharedAccountQuery()
        {
            return _sharedAccountRepository.Query();
        }

        public IQueryable<Template> TemplateQuery()
        {
            return _templateRepository.Query();
        }

        public IQueryable<Company> CompanyQuery()
        {
            return _companyRepository.Query();
        }

        public IQueryable<Department> DepartmentQuery()
        {
            return _departmentRepository.Query();
        }

        public IQueryable<Position> PositionQuery()
        {
            return _positionRepository.Query();
        }

        public async Task<Employee> EmployeeGetByIdAsync(dynamic id)
        {
            return await _employeeRepository.GetByIdAsync(id);
        }

        public async Task<Device> DeviceGetByIdAsync(dynamic id)
        {
            return await _deviceRepository.GetByIdAsync(id);
        }

        public async Task<DeviceAccount> DeviceAccountGetByIdAsync(dynamic id)
        {
            return await _deviceAccountRepository.GetByIdAsync(id);
        }

        public async Task<DeviceTask> DeviceTaskGetByIdAsync(dynamic id)
        {
            return await _deviceTaskRepository.GetByIdAsync(id);
        }

        public async Task<SharedAccount> SharedAccountGetByIdAsync(dynamic id)
        {
            return await _sharedAccountRepository.GetByIdAsync(id);
        }

        public async Task<Template> TemplateGetByIdAsync(dynamic id)
        {
            return await _templateRepository.GetByIdAsync(id);
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            return await _employeeRepository.AddAsync(employee);
        }

        public async Task EditEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            await _employeeRepository.UpdateAsync(employee);
        }

        public async Task DeleteEmployeeAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
                throw new Exception("Employee not found");

            // Remove all events
            var allEvents = await _workstationEventRepository.Query().Where(e => e.EmployeeId == id).ToListAsync();
            await _workstationEventRepository.DeleteRangeAsync(allEvents);
            // Remove all events
            var allSessions = await _workstationSessionRepository.Query().Where(s => s.EmployeeId == id).ToListAsync();
            await _workstationSessionRepository.DeleteRangeAsync(allSessions);
            // Remove all accounts
            var allAccounts = await _deviceAccountRepository.Query().Where(t => t.EmployeeId == id).ToListAsync();
            await _deviceAccountRepository.DeleteRangeAsync(allAccounts);

            await _employeeRepository.DeleteAsync(employee);
        }

        public async Task<bool> ExistAsync(Expression<Func<Employee, bool>> predicate)
        {
            return await _employeeRepository.ExistAsync(predicate);
        }

        public async Task SetPrimaryAccount(string deviceId, string deviceAccountId)
        {
            if (deviceId == null)
                throw new ArgumentNullException(nameof(deviceId));

            if (deviceAccountId == null)
                throw new ArgumentNullException(nameof(deviceAccountId));

            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
                throw new Exception($"Device not found, ID: {deviceId}");

            // Update Device Account
            var deviceAccount = await _deviceAccountRepository.GetByIdAsync(deviceAccountId);
            if (deviceAccount == null)
                throw new Exception($"DeviceAccount not found, ID: {deviceAccountId}");

            deviceAccount.Status = AccountStatus.Updating;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt" };
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);

            // Add task
            await _remoteTaskService.AddTaskAsync(new DeviceTask()
            {
                Operation = TaskOperation.Primary,
                CreatedAt = DateTime.UtcNow,
                DeviceId = device.Id,
                DeviceAccountId = deviceAccountId
            });

            _remoteTaskService.StartTaskProcessing(deviceId);
        }

        public async Task AddDeviceAsync(string employeeId, string[] selectedDevices)
        {
            _dataProtectionService.Validate();

            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                throw new Exception("Employee not found");

            foreach (var deviceId in selectedDevices)
            {
                var device = await _deviceRepository.GetByIdAsync(deviceId);
                if (device == null)
                    throw new Exception($"Device not found, ID: {deviceId}");

                if (device.EmployeeId != null)
                    throw new Exception($"Device {deviceId} already linked to another employee");

                var masterPassword = GenerateMasterPassword();

                device.EmployeeId = employeeId;
                device.MasterPassword = masterPassword;
                await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "EmployeeId", "MasterPassword" });

                await _remoteTaskService.AddTaskAsync(new DeviceTask
                {
                    Password = masterPassword,
                    Operation = TaskOperation.Link,
                    CreatedAt = DateTime.UtcNow,
                    DeviceId = device.Id
                });

                _remoteTaskService.StartTaskProcessing(deviceId);
            }
        }

        public async Task RemoveDeviceAsync(string employeeId, string deviceId)
        {
            _dataProtectionService.Validate();

            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
                throw new Exception($"Device not found, ID: {deviceId}");

            if (device.EmployeeId != employeeId)
                throw new Exception($"Device {deviceId} not linked to employee");

            // Remove all tasks
            var allTasks = await _deviceTaskRepository.Query().Where(t => t.DeviceId == deviceId).ToListAsync();
            await _deviceTaskRepository.DeleteRangeAsync(allTasks);

            // Remove all accounts
            var allAccounts = await _deviceAccountRepository.Query().Where(d => d.DeviceId == deviceId).ToListAsync();
            foreach (var account in allAccounts)
            {
                account.Deleted = true;
            }
            await _deviceAccountRepository.UpdateOnlyPropAsync(allAccounts, new string[] { "Deleted" });

            // Remove employee from device
            device.EmployeeId = null;
            device.PrimaryAccountId = null;
            await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "EmployeeId", "PrimaryAccountId" });

            await _remoteTaskService.RemoveDeviceAsync(device);

            _remoteTaskService.StartTaskProcessing(deviceId);
        }

        public async Task CreateWorkstationAccountAsync(WorkstationAccountModel workstationAccount, string employeeId, string deviceId)
        {
            if (workstationAccount == null)
            {
                throw new ArgumentNullException(nameof(workstationAccount));
            }
            if (deviceId == null)
            {
                throw new ArgumentNullException(nameof(deviceId));
            }

            var deviceAccount = new DeviceAccount()
            {
                Name = "Workstation Account",
                EmployeeId = employeeId                
            };

            switch (workstationAccount.AccountType)
            {
                case WorkstationAccountType.Local:
                    deviceAccount.Login = $".\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.Domain:
                    deviceAccount.Login = $"{workstationAccount.Domain}\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.Microsoft:
                    deviceAccount.Login = $"@\\{workstationAccount.Login}";
                    break;
            }

            var input = new InputModel()
            {
                Password = workstationAccount.Password
            };

            await CreatePersonalAccountAsync(deviceAccount, input, new string[] { deviceId });
        }

        public async Task CreatePersonalAccountAsync(DeviceAccount deviceAccount, InputModel input, string[] selectedDevices)
        {
            if (deviceAccount == null)
                throw new ArgumentNullException(nameof(deviceAccount));

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (selectedDevices == null)
                throw new ArgumentNullException(nameof(selectedDevices));

            _dataProtectionService.Validate();

            List<DeviceAccount> accounts = new List<DeviceAccount>();
            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceId in selectedDevices)
            {
                var exist = await _deviceAccountRepository
                    .Query()
                    .Where(s => s.Name == deviceAccount.Name)
                    .Where(s => s.Login == deviceAccount.Login)
                    .Where(s => s.Deleted == false)
                    .Where(s => s.DeviceId == deviceId)
                    .AnyAsync();

                if (exist)
                    throw new Exception("An account with the same name and login exists.");

                // Validate url
                if (deviceAccount.Urls != null)
                {
                    List<string> verifiedUrls = new List<string>();
                    foreach (var url in deviceAccount.Urls.Split(";"))
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
                    deviceAccount.Urls = string.Join(";", verifiedUrls.ToArray());
                }

                var deviceAccountId = Guid.NewGuid().ToString();

                // Create Device Account
                accounts.Add(new DeviceAccount
                {
                    Id = deviceAccountId,
                    Name = deviceAccount.Name,
                    Urls = deviceAccount.Urls,
                    Apps = deviceAccount.Apps,
                    Login = deviceAccount.Login,
                    Type = AccountType.Personal,
                    Status = AccountStatus.Creating,
                    CreatedAt = DateTime.UtcNow,
                    PasswordUpdatedAt = DateTime.UtcNow,
                    OtpUpdatedAt = input.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null,
                    EmployeeId = deviceAccount.EmployeeId,
                    DeviceId = deviceId,
                    SharedAccountId = null
                });

                // Create Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccountId,
                    Name = deviceAccount.Name,
                    Urls = deviceAccount.Urls,
                    Apps = deviceAccount.Apps,
                    Login = deviceAccount.Login,
                    Password = _dataProtectionService.Protect(input.Password),
                    OtpSecret = input.OtpSecret,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Create,
                    DeviceId = deviceId
                });

                // Set primary account
                await SetAsPrimaryIfEmpty(deviceId, deviceAccountId);
            }

            await _deviceAccountRepository.AddRangeAsync(accounts);

            try
            {
                await _remoteTaskService.AddRangeAsync(tasks);
            }
            catch (Exception)
            {
                await _deviceAccountRepository.DeleteRangeAsync(accounts);
                throw;
            }

            _remoteTaskService.StartTaskProcessing(selectedDevices);
        }

        public async Task EditPersonalAccountAsync(DeviceAccount deviceAccount)
        {
            _dataProtectionService.Validate();

            if (deviceAccount == null)
                throw new ArgumentNullException(nameof(deviceAccount));

            var exist = await _deviceAccountRepository
                .Query()
                .Where(s => s.Name == deviceAccount.Name)
                .Where(s => s.Login == deviceAccount.Login)
                .Where(s => s.Deleted == false)
                .Where(s => s.Id != deviceAccount.Id)
                .Where(s => s.DeviceId == deviceAccount.DeviceId)
                .AnyAsync();

            if (exist)
                throw new Exception("An account with the same name and login exists.");

            // Validate url
            if (deviceAccount.Urls != null)
            {
                List<string> verifiedUrls = new List<string>();
                foreach (var url in deviceAccount.Urls.Split(";"))
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
                deviceAccount.Urls = string.Join(";", verifiedUrls.ToArray());
            }

            // Update Device Account
            deviceAccount.Status = AccountStatus.Updating;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt" };
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);

            // Create Device Task
            try
            {
                await _remoteTaskService.AddTaskAsync(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    Name = deviceAccount.Name,
                    Urls = deviceAccount.Urls ?? string.Empty,
                    Apps = deviceAccount.Apps ?? string.Empty,
                    Login = deviceAccount.Login,
                    Password = null,
                    OtpSecret = null,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }
            catch (Exception)
            {
                deviceAccount.Status = AccountStatus.Error;
                await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
                throw;
            }

            _remoteTaskService.StartTaskProcessing(deviceAccount.DeviceId);
        }

        public async Task EditPersonalAccountPwdAsync(DeviceAccount deviceAccount, InputModel input)
        {
            _dataProtectionService.Validate();

            if (deviceAccount == null)
                throw new ArgumentNullException(nameof(deviceAccount));

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            // Update Device Account
            deviceAccount.Status = AccountStatus.Updating;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt" };
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);

            // Create Device Task
            try
            {
                await _remoteTaskService.AddTaskAsync(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    Password = _dataProtectionService.Protect(input.Password),
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }
            catch (Exception)
            {
                deviceAccount.Status = AccountStatus.Error;
                await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
                throw;
            }

            _remoteTaskService.StartTaskProcessing(deviceAccount.DeviceId);
        }

        public async Task EditPersonalAccountOtpAsync(DeviceAccount deviceAccount, InputModel input)
        {
            _dataProtectionService.Validate();

            if (deviceAccount == null)
                throw new ArgumentNullException(nameof(deviceAccount));

            // Update Device Account
            deviceAccount.Status = AccountStatus.Updating;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            deviceAccount.OtpUpdatedAt = DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt", "OtpUpdatedAt" };
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);

            // Create Device Task
            try
            {
                await _remoteTaskService.AddTaskAsync(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    OtpSecret = _dataProtectionService.Protect(input.OtpSecret ?? string.Empty),
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }
            catch (Exception)
            {
                deviceAccount.Status = AccountStatus.Error;
                await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
                throw;
            }

            _remoteTaskService.StartTaskProcessing(deviceAccount.DeviceId);
        }

        public async Task AddSharedAccount(string employeeId, string sharedAccountId, string[] selectedDevices)
        {
            _dataProtectionService.Validate();

            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (sharedAccountId == null)
                throw new ArgumentNullException(nameof(sharedAccountId));

            if (selectedDevices == null)
                throw new ArgumentNullException(nameof(selectedDevices));


            List<DeviceAccount> accounts = new List<DeviceAccount>();
            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceId in selectedDevices)
            {
                // Get Shared Account
                var sharedAccount = await _sharedAccountRepository.GetByIdAsync(sharedAccountId);
                if (sharedAccount == null)
                    throw new Exception("SharedAccount not found");

                var exist = await _deviceAccountRepository
                    .Query()
                    .Where(s => s.Name == sharedAccount.Name)
                    .Where(s => s.Login == sharedAccount.Login)
                    .Where(s => s.Deleted == false)
                    .Where(d => d.DeviceId == deviceId)
                    .AnyAsync();

                if (exist)
                    throw new Exception("An account with the same name and login exists");

                // Create Device Account
                var deviceAccountId = Guid.NewGuid().ToString();
                accounts.Add(new DeviceAccount
                {
                    Id = deviceAccountId,
                    Name = sharedAccount.Name,
                    Urls = sharedAccount.Urls,
                    Apps = sharedAccount.Apps,
                    Login = sharedAccount.Login,
                    Type = AccountType.Shared,
                    Status = AccountStatus.Creating,
                    CreatedAt = DateTime.UtcNow,
                    PasswordUpdatedAt = DateTime.UtcNow,
                    OtpUpdatedAt = sharedAccount.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null,
                    EmployeeId = employeeId,
                    DeviceId = deviceId,
                    SharedAccountId = sharedAccountId
                });

                // Create Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccountId,
                    Name = sharedAccount.Name,
                    Urls = sharedAccount.Urls,
                    Apps = sharedAccount.Apps,
                    Login = sharedAccount.Login,
                    Password = sharedAccount.Password,
                    OtpSecret = sharedAccount.OtpSecret,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Create,
                    DeviceId = deviceId
                });

                // Set primary account
                await SetAsPrimaryIfEmpty(deviceId, deviceAccountId);
            }

            await _deviceAccountRepository.AddRangeAsync(accounts);
            try
            {
                await _remoteTaskService.AddRangeAsync(tasks);
            }
            catch (Exception)
            {
                await _deviceAccountRepository.DeleteRangeAsync(accounts);
                throw;
            }

            _remoteTaskService.StartTaskProcessing(selectedDevices);
        }

        public async Task DeleteAccount(string accountId)
        {
            _dataProtectionService.Validate();

            if (accountId == null)
                throw new ArgumentNullException(nameof(accountId));

            var deviceAccount = await _deviceAccountRepository.GetByIdAsync(accountId);
            if (deviceAccount == null)
                throw new Exception("Device account not found");

            // Update Device Account
            deviceAccount.Status = AccountStatus.Removing;
            deviceAccount.UpdatedAt = DateTime.UtcNow;
            string[] properties = { "Status", "UpdatedAt" };
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);

            try
            {
                // Create Device Task
                await _remoteTaskService.AddTaskAsync(new DeviceTask
                {
                    DeviceAccountId = accountId,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Delete,
                    DeviceId = deviceAccount.DeviceId
                });
            }
            catch (Exception)
            {
                deviceAccount.Status = AccountStatus.Error;
                await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
                throw;
            }

            _remoteTaskService.StartTaskProcessing(deviceAccount.DeviceId);
        }

        public async Task UndoChanges(string accountId)
        {
            if (accountId == null)
                throw new ArgumentNullException(nameof(accountId));

            _dataProtectionService.Validate();

            await _remoteTaskService.UndoLastTaskAsync(accountId);
        }

        private async Task SetAsPrimaryIfEmpty(string deviceId, string deviceAccountId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);

            if (device.PrimaryAccountId == null)
            {
                device.PrimaryAccountId = deviceAccountId;
                await _deviceRepository.UpdateOnlyPropAsync(device, new string[] { "PrimaryAccountId" });
            }
        }

        private string GenerateMasterPassword()
        {
            var buf = AesCryptoHelper.CreateRandomBuf(32);
            var pass = ConvertUtils.ByteArrayToHexString(buf);
            return pass;
        }
    }
}