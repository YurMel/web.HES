using HES.Core.Entities;
using HES.Core.Entities.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmployeeService
    {
        IQueryable<Employee> EmployeeQuery();
        IQueryable<Device> DeviceQuery();
        IQueryable<DeviceAccount> DeviceAccountQuery();
        IQueryable<DeviceTask> DeviceTaskQuery();
        IQueryable<SharedAccount> SharedAccountQuery();
        IQueryable<Template> TemplateQuery();
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        IQueryable<Position> PositionQuery();
        Task<Employee> EmployeeGetByIdAsync(dynamic id);
        Task<Device> DeviceGetByIdAsync(dynamic id);
        Task<DeviceAccount> DeviceAccountGetByIdAsync(dynamic id);
        Task<DeviceTask> DeviceTaskGetByIdAsync(dynamic id);
        Task<SharedAccount> SharedAccountGetByIdAsync(dynamic id);
        Task<Template> TemplateGetByIdAsync(dynamic id);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string id);
        Task<bool> ExistAsync(Expression<Func<Employee, bool>> predicate);
        Task EnableSamlIdpAsync(Employee employee);
        Task CreateSamlIdpAccountAsync(string email, string password, string hesUrl);
        Task UpdatePasswordSamlIdpAccountAsync(string email, string password);
        Task UpdateOtpSamlIdpAccountAsync(string email, string otp);
        Task UpdateUrlSamlIdpAccountAsync(string hesUrl);
        Task DeleteSamlIdpAccountAsync(string employeeId);
        Task SetPrimaryAccount(string deviceId, string deviceAccountId);
        Task AddDeviceAsync(string employeeId, string[] selectedDevices);
        Task RemoveDeviceAsync(string employeeId, string deviceId);
        Task CreateWorkstationAccountAsync(WorkstationAccountModel workstationAccount, string employeeId, string deviceId);
        Task CreatePersonalAccountAsync(DeviceAccount deviceAccount, InputModel input, string[] selectedDevices);
        Task EditPersonalAccountAsync(DeviceAccount deviceAccount);
        Task EditPersonalAccountPwdAsync(DeviceAccount deviceAccount, InputModel input);
        Task EditPersonalAccountOtpAsync(DeviceAccount deviceAccount, InputModel input);
        Task AddSharedAccount(string employeeId, string sharedAccountId, string[] selectedDevices);
        Task DeleteAccount(string accountId);
        Task UndoChanges(string accountId);
    }
}