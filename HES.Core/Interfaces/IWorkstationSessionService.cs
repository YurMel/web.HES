using HES.Core.Entities;
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
        Task AddSessionAsync(WorkstationSession workstationSession);
        Task UpdateWorkstationSessions(IList<WorkstationEvent> events);
    }
}