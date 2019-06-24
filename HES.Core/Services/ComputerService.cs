using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class ComputerService : IComputerService
    {
        private readonly IAsyncRepository<Computer> _computerRepository;
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IAsyncRepository<Department> _departmentRepository;

        public ComputerService(IAsyncRepository<Computer> computerRepository,
                               IAsyncRepository<Company> companyRepository,
                               IAsyncRepository<Department> departmentRepository)
        {
            _computerRepository = computerRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
        }

        public IQueryable<Computer> ComputerQuery()
        {
            return _computerRepository.Query();
        }

        public IQueryable<Company> CompanyQuery()
        {
            return _companyRepository.Query();
        }

        public IQueryable<Department> DepartmentQuery()
        {
            return _departmentRepository.Query();
        }

        public async Task EditDepartmentAsync(Computer computer)
        {
            if (computer == null)
                throw new ArgumentNullException(nameof(computer));

            computer.CompanyId = computer.Department.CompanyId;

            string[] properties = { "CompanyId", "DepartmentId" };
            await _computerRepository.UpdateOnlyPropAsync(computer, properties);
        }

        public async Task ApproveComputerAsync(string computerId)
        {
            if (computerId == null)
                throw new ArgumentNullException(nameof(computerId));

            var computer = await _computerRepository.GetByIdAsync(computerId);
            if (computer == null)
                throw new Exception("Computer not found");

            computer.Approved = true;

            string[] properties = { "Approved" };
            await _computerRepository.UpdateOnlyPropAsync(computer, properties);
        }
    }
}
