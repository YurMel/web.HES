﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationEventService : IWorkstationEventService
    {
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<Workstation> _workstationRepository;
        private readonly IAsyncRepository<WorkstationProximityDevice> _workstationBindingRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IAsyncRepository<Department> _departmentRepository;
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;

        public WorkstationEventService(IAsyncRepository<WorkstationEvent> workstationEventRepository,
                                       IAsyncRepository<Workstation> workstationRepository,
                                       IAsyncRepository<WorkstationProximityDevice> workstationBindingRepository,
                                       IAsyncRepository<Device> deviceRepository,
                                       IAsyncRepository<Employee> employeeRepository,
                                       IAsyncRepository<Company> companyRepository,
                                       IAsyncRepository<Department> departmentRepository,
                                       IAsyncRepository<DeviceAccount> deviceAccountRepository)
        {
            _workstationEventRepository = workstationEventRepository;
            _workstationRepository = workstationRepository;
            _workstationBindingRepository = workstationBindingRepository;
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
            _deviceAccountRepository = deviceAccountRepository;
        }

        public IQueryable<WorkstationEvent> WorkstationEventQuery()
        {
            return _workstationEventRepository.Query();
        }

        public IQueryable<Workstation> WorkstationQuery()
        {
            return _workstationRepository.Query();
        }

        public IQueryable<Device> DeviceQuery()
        {
            return _deviceRepository.Query();
        }

        public IQueryable<Employee> EmployeeQuery()
        {
            return _employeeRepository.Query();
        }

        public IQueryable<Company> CompanyQuery()
        {
            return _companyRepository.Query();
        }

        public IQueryable<Department> DepartmentQuery()
        {
            return _departmentRepository.Query();
        }

        public IQueryable<DeviceAccount> DeviceAccountQuery()
        {
            return _deviceAccountRepository.Query();
        }

        public async Task AddEventAsync(WorkstationEvent workstationEvent)
        {
            if (workstationEvent == null)
                throw new ArgumentNullException(nameof(workstationEvent));

            await _workstationEventRepository.AddAsync(workstationEvent);
        }

        public async Task<IEnumerable<WorkstationEvent>> AddEventsRangeAsync(IList<WorkstationEvent> workstationEvents)
        {
            if (workstationEvents == null)
                throw new ArgumentNullException(nameof(workstationEvents));

            // Get EmployeeId, DepartmentId, DeviceAccountId based on DeviceId and other information from event
            foreach (var e in workstationEvents)
            {
                // Skip events for workstations that are not present in DB
                var workstationExist = await _workstationRepository.ExistAsync(w => w.Id == e.WorkstationId);
                if (!workstationExist)
                    continue;

                e.EmployeeId = _employeeRepository.Query()
                    .Include(employee => employee.Devices)
                    .AsNoTracking()
                    .FirstOrDefault(employee => employee.Devices.Any(d => d.Id == e.DeviceId))?.Id;

                e.DepartmentId = _employeeRepository.Query()
                    .AsNoTracking()
                    .FirstOrDefault(employee => employee.Id == e.EmployeeId)?.DepartmentId;

                if (e.DeviceAccount != null)
                {
                    e.DeviceAccountId = _deviceAccountRepository.Query()
                        .AsNoTracking()
                        .FirstOrDefault(da => da.EmployeeId == e.EmployeeId
                        && da.DeviceId == e.DeviceId
                        && da.Name == e.DeviceAccount.Name
                        && da.Login == e.DeviceAccount.Login)?.Id;
                }
            }

            return await _workstationEventRepository.AddRangeAsync(workstationEvents);
        }
    }
}