using HES.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IComputerService
    {
        IQueryable<Computer> ComputerQuery();
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        Task EditDepartmentAsync(Computer computer);
        Task ApproveComputerAsync(string id);
    }
}