using HES.Core.Entities;
using System;
using System.Collections.Generic;
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
        IQueryable<Department> DepartmentQuery();
        IQueryable<Position> PositionQuery();
        Task<Employee> EmployeeGetByIdAsync(dynamic id);
        Task<Device> DeviceGetByIdAsync(dynamic id);
        Task<DeviceAccount> DeviceAccountGetByIdAsync(dynamic id);
        Task<DeviceTask> DeviceTaskGetByIdAsync(dynamic id);
        Task<SharedAccount> SharedAccountGetByIdAsync(dynamic id);
        Task<Template> TemplateGetByIdAsync(dynamic id);
        //Task<IList<Employee>> GetAllAsync();
        //Task<IList<Employee>> GetAllWhereAsync(Expression<Func<Employee, bool>> predicate);
        //Task<IList<Employee>> GetAllIncludeAsync(params Expression<Func<Employee, object>>[] navigationProperties);
        //Task<Employee> GetFirstOrDefaulAsync();
        //Task<Employee> GetFirstOrDefaulAsync(Expression<Func<Employee, bool>> match);
        //Task<Employee> GetFirstOrDefaulIncludeAsync(Expression<Func<Employee, bool>> where, params Expression<Func<Employee, object>>[] navigationProperties);
        //Task<Employee> GetByIdAsync(dynamic id);
        Task CreateEmployeeAsync(Employee employee);
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string id);
        bool Exist(Expression<Func<Employee, bool>> predicate);
        Task AddDeviceAsync(string employeeId, string[] selectedDevices);
        Task RemoveDeviceAsync(string employeeId, string deviceId);
        Task CreatePersonalAccountAsync(DeviceAccount deviceAccount, InputModel input, string[] selectedDevices);
        Task EditPersonalAccountAsync(DeviceAccount deviceAccount);
        Task EditPersonalAccountPwdAsync(DeviceAccount deviceAccount, InputModel input);
        Task EditPersonalAccountOtpAsync(DeviceAccount deviceAccount, InputModel input);
        Task AddSharedAccount(string employeeId, string sharedAccountId, string[] selectedDevices);
        Task DeleteAccount(string accountId);
    }
}