using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IAsyncRepository<Department> _departmentRepository;

        public SettingsService(IAsyncRepository<Department> departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public IQueryable<Department> DepartmentQuery()
        {
           return _departmentRepository.Query();
        }

        public Task CreateDepartment()
        {
            return Task.CompletedTask;
        }
    }
}
