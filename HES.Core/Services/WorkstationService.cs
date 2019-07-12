using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationService : IWorkstationService
    {
        private readonly IAsyncRepository<Workstation> _workstationRepository;
        private readonly IAsyncRepository<WorkstationBinding> _workstationBindingRepository;
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IAsyncRepository<Department> _departmentRepository;

        public WorkstationService(IAsyncRepository<Workstation> workstationRepository,
                                  IAsyncRepository<WorkstationBinding> workstationBindingRepository,
                                  IAsyncRepository<Company> companyRepository,
                                  IAsyncRepository<Department> departmentRepository)
        {
            _workstationRepository = workstationRepository;
            _workstationBindingRepository = workstationBindingRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
        }

        public IQueryable<Workstation> WorkstationQuery()
        {
            return _workstationRepository.Query();
        }

        public IQueryable<WorkstationBinding> WorkstationBindingQuery()
        {
            return _workstationBindingRepository.Query();
        }

        public IQueryable<Company> CompanyQuery()
        {
            return _companyRepository.Query();
        }

        public IQueryable<Department> DepartmentQuery()
        {
            return _departmentRepository.Query();
        }

        public bool Exist(Expression<Func<Workstation, bool>> predicate)
        {
            return _workstationRepository.Exist(predicate);
        }

        public async Task AddWorkstationAsync(Workstation workstation)
        {
            if (workstation == null)
                throw new ArgumentNullException(nameof(workstation));

            await _workstationRepository.AddAsync(workstation);
        }

        public async Task UpdateClientVersionAsync(string workstationId, string clientVersion)
        {
            if (workstationId == null)
                throw new ArgumentNullException(nameof(workstationId));

            var workstation = await _workstationRepository.GetByIdAsync(workstationId);
            if (workstation == null)
                throw new Exception($"Workstation not found, ID: {workstation}");

            workstation.ClientVersion = clientVersion;

            string[] properties = { "ClientVersion" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task UpdateOsAsync(string workstationId, string os)
        {
            if (workstationId == null)
                throw new ArgumentNullException(nameof(workstationId));

            var workstation = await _workstationRepository.GetByIdAsync(workstationId);
            if (workstation == null)
                throw new Exception($"Workstation not found, ID: {workstation}");

            workstation.OS = os;

            string[] properties = { "OS" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task UpdateIpAsync(string workstationId, string ip)
        {
            if (workstationId == null)
                throw new ArgumentNullException(nameof(workstationId));

            var workstation = await _workstationRepository.GetByIdAsync(workstationId);
            if (workstation == null)
                throw new Exception($"Workstation not found, ID: {workstation}");

            workstation.IP = ip;

            string[] properties = { "IP" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task UpdateLastSeenAsync(string workstationId)
        {
            if (workstationId == null)
                throw new ArgumentNullException(nameof(workstationId));

            var workstation = await _workstationRepository.GetByIdAsync(workstationId);
            if (workstation == null)
                throw new Exception($"Workstation not found, ID: {workstation}");

            workstation.LastSeen = DateTime.UtcNow;

            string[] properties = { "LastSeen" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task EditDepartmentAsync(Workstation workstation)
        {
            if (workstation == null)
                throw new ArgumentNullException(nameof(workstation));

            string[] properties = { "DepartmentId" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task ApproveWorkstationAsync(string workstationId)
        {
            if (workstationId == null)
                throw new ArgumentNullException(nameof(workstationId));

            var workstation = await _workstationRepository.GetByIdAsync(workstationId);
            if (workstation == null)
                throw new Exception("Workstation not found");

            workstation.Approved = true;

            string[] properties = { "Approved" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task UnapproveWorkstationAsync(string workstationId)
        {
            if (workstationId == null)
                throw new ArgumentNullException(nameof(workstationId));

            var workstation = await _workstationRepository.GetByIdAsync(workstationId);
            if (workstation == null)
                throw new Exception("Workstation not found");

            workstation.Approved = false;

            string[] properties = { "Approved" };
            await _workstationRepository.UpdateOnlyPropAsync(workstation, properties);
        }

        public async Task AddBindingAsync(string workstationId, bool allowRfid, bool allowBleTap, bool allowProximity, string[] selectedDevices)
        {
            if (workstationId == null)
            {
                throw new ArgumentNullException(nameof(workstationId));
            }
            if (selectedDevices == null)
            {
                throw new ArgumentNullException(nameof(selectedDevices));
            }

            List<WorkstationBinding> workstationBindings = new List<WorkstationBinding>();

            foreach (var deviceId in selectedDevices)
            {
                var binding = await _workstationBindingRepository
                .Query()
                .Where(d => d.DeviceId == deviceId)
                .Where(d => d.WorkstationId == workstationId)
                .FirstOrDefaultAsync();

                if (binding != null)
                {
                    throw new Exception("Workstation binding already exist.");
                }

                workstationBindings.Add(new WorkstationBinding {
                    WorkstationId = workstationId,
                    DeviceId = deviceId,
                    AllowRfid = allowRfid,
                    AllowBleTap = allowBleTap,
                    AllowProximity = allowProximity
                });
            }
            
            await _workstationBindingRepository.AddRangeAsync(workstationBindings);
        }

        public async Task EditBindingAsync(WorkstationBinding workstationBinding)
        {
            if (workstationBinding == null)
                throw new ArgumentNullException(nameof(workstationBinding));

            string[] properties = { "AllowRfid", "AllowBleTap", "AllowProximity" };
            await _workstationBindingRepository.UpdateOnlyPropAsync(workstationBinding, properties);
        }

        public async Task DeleteBindingAsync(string workstationBindingId)
        {
            if (workstationBindingId == null)
            {
                throw new ArgumentNullException(nameof(workstationBindingId));
            }

            var binding = await _workstationBindingRepository.GetByIdAsync(workstationBindingId);
            if (binding == null)
            {
                throw new Exception("Binding not found.");
            }

            await _workstationBindingRepository.DeleteAsync(binding);
        }
    }
}
