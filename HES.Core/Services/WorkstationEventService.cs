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
        private readonly IAsyncRepository<WorkstationProximityDevice> _workstationProximityDeviceRepository;
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
            _workstationProximityDeviceRepository = workstationBindingRepository;
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
            _deviceAccountRepository = deviceAccountRepository;
        }

        public IQueryable<WorkstationEvent> Query()
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
            foreach (var workstationEvent in workstationEvents)
            {
                // Skip events for workstations that are not present in DB
                var workstationExist = await _workstationRepository.ExistAsync(w => w.Id == workstationEvent.WorkstationId);
                if (!workstationExist)
                    continue;

                workstationEvent.EmployeeId = _employeeRepository.Query()
                    .Include(employee => employee.Devices)
                    .AsNoTracking()
                    .FirstOrDefault(employee => employee.Devices.Any(d => d.Id == workstationEvent.DeviceId))?.Id;

                workstationEvent.DepartmentId = _employeeRepository.Query()
                    .AsNoTracking()
                    .FirstOrDefault(employee => employee.Id == workstationEvent.EmployeeId)?.DepartmentId;

                if (workstationEvent.DeviceAccount != null)
                {
                    workstationEvent.DeviceAccountId = _deviceAccountRepository.Query()
                        .AsNoTracking()
                        .FirstOrDefault(da => da.EmployeeId == workstationEvent.EmployeeId
                        && da.DeviceId == workstationEvent.DeviceId
                        && da.Name == workstationEvent.DeviceAccount.Name
                        && da.Login == workstationEvent.DeviceAccount.Login)?.Id;
                }
            }

            return await _workstationEventRepository.AddRangeAsync(workstationEvents);
        }
    }
}