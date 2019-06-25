using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationService : IWorkstationService
    {
        private readonly IAsyncRepository<Workstation> _workstationRepository;
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IAsyncRepository<Department> _departmentRepository;

        public WorkstationService(IAsyncRepository<Workstation> workstationRepository,
                               IAsyncRepository<Company> companyRepository,
                               IAsyncRepository<Department> departmentRepository)
        {
            _workstationRepository = workstationRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
        }

        public IQueryable<Workstation> WorkstationQuery()
        {
            return _workstationRepository.Query();
        }

        public IQueryable<Company> CompanyQuery()
        {
            return _companyRepository.Query();
        }

        public IQueryable<Department> DepartmentQuery()
        {
            return _departmentRepository.Query();
        }

        public async Task EditDepartmentAsync(Workstation computer)
        {
            if (computer == null)
                throw new ArgumentNullException(nameof(computer));

            computer.CompanyId = computer.Department.CompanyId;

            string[] properties = { "CompanyId", "DepartmentId" };
            await _workstationRepository.UpdateOnlyPropAsync(computer, properties);
        }

        public async Task ApproveWorkstationAsync(string computerId)
        {
            if (computerId == null)
                throw new ArgumentNullException(nameof(computerId));

            var computer = await _workstationRepository.GetByIdAsync(computerId);
            if (computer == null)
                throw new Exception("Computer not found");

            computer.Approved = true;

            string[] properties = { "Approved" };
            await _workstationRepository.UpdateOnlyPropAsync(computer, properties);
        }
    }
}
