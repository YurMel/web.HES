using HES.Core.Entities;
using HES.Core.Entities.Models;
using Hideez.SDK.Communication.HES.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationSessionService
    {
        IQueryable<WorkstationSession> WorkstationSessionQuery();
        IQueryable<Workstation> WorkstationQuery();
        IQueryable<Device> DeviceQuery();
        IQueryable<Employee> EmployeeQuery();
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        IQueryable<DeviceAccount> DeviceAccountQuery();
        IQueryable<SummaryByDayAndEmployee> SummaryByDayAndEmployeeSqlQuery(string sql);
        IQueryable<SummaryByEmployees> SummaryByEmployeesSqlQuery(string sql);
        IQueryable<SummaryByDepartments> SummaryByDepartmentsSqlQuery(string sql);
        IQueryable<SummaryByWorkstations> SummaryByWorkstationsSqlQuery(string sql);
        Task AddOrUpdateWorkstationSessions(IList<WorkstationEventDto> workstationEventsDto);
        Task AddSessionAsync(WorkstationEventDto workstationEventDto);
        Task UpdateSessionAsync(WorkstationSession workstationSession);
    }
}