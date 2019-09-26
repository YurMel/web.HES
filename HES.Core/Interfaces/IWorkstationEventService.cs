using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationEventService
    {
        IQueryable<WorkstationEvent> Query();
        IQueryable<Workstation> WorkstationQuery();
        IQueryable<Device> DeviceQuery();
        IQueryable<Employee> EmployeeQuery();
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        IQueryable<DeviceAccount> DeviceAccountQuery();
        Task AddEventAsync(WorkstationEvent workstationEvent);
        Task<IEnumerable<WorkstationEvent>> AddEventsRangeAsync(IList<WorkstationEvent> workstationEvents);
    }
}