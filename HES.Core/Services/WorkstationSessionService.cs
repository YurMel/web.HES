using HES.Core.Entities;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkstationEvent = HES.Core.Entities.WorkstationEvent;

namespace HES.Core.Services
{
    public class WorkstationSessionService : IWorkstationSessionService
    {
        private readonly IAsyncRepository<WorkstationSession> _workstationSessionRepository;
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;
        private readonly IAsyncRepository<Workstation> _workstationRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<Company> _companyRepository;
        private readonly IAsyncRepository<Department> _departmentRepository;
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;

        public WorkstationSessionService(IAsyncRepository<WorkstationSession> workstationSessionRepository,
                                       IAsyncRepository<WorkstationEvent> workstationEventRepository,
                                       IAsyncRepository<Workstation> workstationRepository,
                                       IAsyncRepository<Device> deviceRepository,
                                       IAsyncRepository<Employee> employeeRepository,
                                       IAsyncRepository<Company> companyRepository,
                                       IAsyncRepository<Department> departmentRepository,
                                       IAsyncRepository<DeviceAccount> deviceAccountRepository)
        {
            _workstationSessionRepository = workstationSessionRepository;
            _workstationEventRepository = workstationEventRepository;
            _workstationRepository = workstationRepository;
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
            _deviceAccountRepository = deviceAccountRepository;
        }

        public IQueryable<WorkstationSession> WorkstationSessionQuery()
        {
            return _workstationSessionRepository.Query();
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

        public async Task AddSessionAsync(WorkstationSession workstationSession)
        {
            if (workstationSession == null)
                throw new ArgumentNullException(nameof(workstationSession));

            await _workstationSessionRepository.AddAsync(workstationSession);
        }

        public async Task UpdateWorkstationSessions(IList<WorkstationEvent> events)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));
            
            foreach (var e in events)
            {
                var lastSession = _workstationSessionRepository.Query()
                        .AsNoTracking()
                        .LastOrDefault(s => s.EndTime == DateTime.MinValue
                        && s.WorkstationId == e.WorkstationId);

                if (e.EventId == WorkstationEventId.ComputerLock || e.EventId == WorkstationEventId.ComputerLogoff)
                {
                    if (lastSession == null)
                    {
                        // Todo: Sessions with 00:00 duration are confusing
                        // There is no unfinished sessions for current workstation
                        //var newSession = CreateSessionFromEvent(e);
                        //newSession.EndTime = newSession.StartTime;
                        //await _workstationSessionRepository.AddAsync(newSession);

                        // Todo: add warning: closed a session when there were no open sessions

                        continue;
                    }
                    else
                    {
                        lastSession.EndTime = e.Date;
                        await _workstationSessionRepository.UpdateAsync(lastSession);
                        continue;
                    }
                }

                if (e.EventId == WorkstationEventId.ComputerLogon || e.EventId == WorkstationEventId.ComputerUnlock)
                { 
                    if (lastSession != null)
                    {
                        // There is an unfinished session for current workstation
                        lastSession.EndTime = e.Date;
                        await _workstationSessionRepository.UpdateAsync(lastSession);

                        // Todo: add warning notification: created a new session while the previous one was still active
                    }

                    var newSession = CreateSessionFromEvent(e);
                    await _workstationSessionRepository.AddAsync(newSession);
                    continue;
                }
            }
        }

        WorkstationSession CreateSessionFromEvent(WorkstationEvent workstationEvent)
        {
            Enum.TryParse(typeof(SessionSwitchSubject), workstationEvent.Note, out object unlockMethod);
            SessionSwitchSubject unlockedBy = unlockMethod == null ? SessionSwitchSubject.NonHideez : (SessionSwitchSubject)unlockMethod;

            return new WorkstationSession()
            {
                StartTime = workstationEvent.Date,
                EndTime = DateTime.MinValue,
                UnlockedBy = unlockedBy,
                WorkstationId = workstationEvent.WorkstationId,
                DeviceId = workstationEvent.DeviceId,
                EmployeeId = workstationEvent.EmployeeId,
                DepartmentId = workstationEvent.DepartmentId,
                DeviceAccountId = workstationEvent.DeviceAccountId,
                UserSession = workstationEvent.UserSession,
            };
        }
    }
}