using HES.Core.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationService
    {
        IQueryable<Workstation> WorkstationQuery();
        IQueryable<WorkstationBinding> WorkstationBindingQuery();
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        bool Exist(Expression<Func<Workstation, bool>> predicate);
        Task AddWorkstationAsync(Workstation workstation);
        Task UpdateClientVersionAsync(string workstationId, string clientVersion);
        Task UpdateOsAsync(string workstationId, string os);
        Task UpdateIpAsync(string workstationId, string os);
        Task UpdateLastSeenAsync(string workstationId);
        Task EditDepartmentAsync(Workstation workstation);
        Task ApproveWorkstationAsync(string id);
        Task UnapproveWorkstationAsync(string id);
        Task AddBindingAsync(string workstationId, bool allowRfid, bool allowBleTap, bool allowProximity, string[] selectedDevices);
        Task EditBindingAsync(WorkstationBinding workstationBinding);
        Task DeleteBindingAsync(string workstationBindingId);
    }
}