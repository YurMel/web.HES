using HES.Core.Entities;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Workstation;
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

        public async Task<bool> ExistAsync(Expression<Func<Workstation, bool>> predicate)
        {
            return await _workstationRepository.ExistAsync(predicate);
        }

        public async Task AddWorkstationAsync(WorkstationInfo workstationInfo)
        {
            if (workstationInfo == null)
                throw new ArgumentNullException(nameof(workstationInfo));

            var workstation = new Workstation()
            {
                Id = workstationInfo.Id,
                Name = workstationInfo.MachineName,
                Domain = workstationInfo.Domain,
                OS = workstationInfo.OsName,
                ClientVersion = workstationInfo.AppVersion,
                IP = workstationInfo.IP,
                LastSeen = DateTime.UtcNow,
                DepartmentId = null
            };

            await _workstationRepository.AddAsync(workstation);
        }

        public async Task UpdateWorkstationAsync(WorkstationInfo workstationInfo)
        {
            if (workstationInfo == null)
                throw new ArgumentNullException(nameof(workstationInfo));

            var workstation = await _workstationRepository.GetByIdAsync(workstationInfo.Id);
            if (workstation == null)
                throw new Exception($"Workstation not found, ID: {workstation}");

            workstation.ClientVersion = workstationInfo.AppVersion;
            workstation.OS = workstationInfo.OsName;
            workstation.IP = workstationInfo.IP;
            workstation.LastSeen = DateTime.UtcNow;

            string[] properties = { "ClientVersion", "OS", "IP", "LastSeen" };
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
            await UpdateWorkstationUnlockerSettingsAsync(workstationId);
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
            await UpdateWorkstationUnlockerSettingsAsync(workstationId);
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

                workstationBindings.Add(new WorkstationBinding
                {
                    WorkstationId = workstationId,
                    DeviceId = deviceId,
                    AllowRfid = allowRfid,
                    AllowBleTap = allowBleTap,
                    AllowProximity = allowProximity
                });
            }

            await _workstationBindingRepository.AddRangeAsync(workstationBindings);
            await UpdateWorkstationUnlockerSettingsAsync(workstationId);
        }

        public async Task AddMultipleBindingAsync(string[] workstationsId, bool allowRfid, bool allowBleTap, bool allowProximity, string[] devicesId)
        {
            if (workstationsId == null)
            {
                throw new ArgumentNullException(nameof(workstationsId));
            }
            if (devicesId == null)
            {
                throw new ArgumentNullException(nameof(devicesId));
            }

            foreach (var workstation in workstationsId)
            {
                await AddBindingAsync(workstation, allowRfid, allowBleTap, allowProximity, devicesId);
            }
        }

        public async Task EditBindingAsync(WorkstationBinding workstationBinding)
        {
            if (workstationBinding == null)
                throw new ArgumentNullException(nameof(workstationBinding));

            string[] properties = { "AllowRfid", "AllowBleTap", "AllowProximity" };
            await _workstationBindingRepository.UpdateOnlyPropAsync(workstationBinding, properties);
            await UpdateWorkstationUnlockerSettingsAsync(workstationBinding.WorkstationId);
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
            await UpdateWorkstationUnlockerSettingsAsync(binding.WorkstationId);
        }

        public async Task<UnlockerSettingsInfo> GetWorkstationUnlockerSettingsInfoAsync(string workstationId)
        {
            var workstation = await _workstationRepository
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == workstationId);

            if (workstation == null)
            {
                throw new Exception("Workstation not found");
            }

            var deviceUnlockerSettings = new List<DeviceUnlockerSettingsInfo>();

            var bindings = await _workstationBindingRepository
                .Query()
                .Include(i => i.Device)
                .Where(b => b.WorkstationId == workstationId)
                .AsNoTracking()
                .ToListAsync();

            if (workstation.Approved)
            {
                foreach (var binding in bindings)
                {
                    deviceUnlockerSettings.Add(new DeviceUnlockerSettingsInfo()
                    {
                        Mac = binding.Device.MAC,
                        AllowRfid = binding.AllowRfid,
                        AllowBleTap = binding.AllowBleTap,
                        AllowProximity = binding.AllowProximity,
                        SerialNo = binding.DeviceId,
                        RequirePin = binding.Device.UsePin,
                    });
                }
            }

            var unlockerSettingsInfo = new UnlockerSettingsInfo()
            {
                LockProximity = 40, //workstation.LockProximity,
                UnlockProximity = 75, //workstation.UnlockProximity,
                LockTimeoutSeconds = 3, //workstation.LockTimeout,
                DeviceUnlockerSettings = deviceUnlockerSettings.ToArray(),
            };

            return unlockerSettingsInfo;
        }

        private async Task UpdateWorkstationUnlockerSettingsAsync(string workstationId)
        {
            var unlockerSettingsInfo = await GetWorkstationUnlockerSettingsInfoAsync(workstationId);

            await AppHub.UpdateUnlockerSettings(workstationId, unlockerSettingsInfo);
        }
    }
}
