using HES.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationService
    {
        IQueryable<Workstation> WorkstationQuery();
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        Task AddWorkstation(Workstation workstation);
        Task EditDepartmentAsync(Workstation workstation);
        Task ApproveWorkstationAsync(string id);
        Task UnapproveWorkstationAsync(string id);
    }
}