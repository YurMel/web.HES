using HES.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationService
    {
        IQueryable<Workstation> WorkstationQuery();
        IQueryable<WorkstationBinding> WorkstationBindingQuery();
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        Task AddWorkstationAsync(Workstation workstation);
        Task EditDepartmentAsync(Workstation workstation);
        Task ApproveWorkstationAsync(string id);
        Task UnapproveWorkstationAsync(string id);
        Task AddBindingAsync(string workstationId, bool allowRfid, bool allowBleTap, bool allowProximity, string[] selectedDevices);
        Task EditBindingAsync(WorkstationBinding workstationBinding);
        Task DeleteBindingAsync(string workstationBindingId);
    }
}