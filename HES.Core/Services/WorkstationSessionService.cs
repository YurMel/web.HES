using HES.Core.Entities;
using HES.Core.Entities.Models;
using HES.Core.Interfaces;
using Hideez.SDK.Communication.WorkstationEvents;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IAsyncRepository<SummaryByDayAndEmployee> _summaryByDayAndEmployeeRepository;
        private readonly IAsyncRepository<SummaryByEmployees> _summaryByEmployeesRepository;
        private readonly IAsyncRepository<SummaryByDepartments> _summaryByDepartmentsRepository;
        private readonly IAsyncRepository<SummaryByWorkstations> _summaryByWorkstationsRepository;

        public WorkstationSessionService(IAsyncRepository<WorkstationSession> workstationSessionRepository,
                                       IAsyncRepository<WorkstationEvent> workstationEventRepository,
                                       IAsyncRepository<Workstation> workstationRepository,
                                       IAsyncRepository<Device> deviceRepository,
                                       IAsyncRepository<Employee> employeeRepository,
                                       IAsyncRepository<Company> companyRepository,
                                       IAsyncRepository<Department> departmentRepository,
                                       IAsyncRepository<DeviceAccount> deviceAccountRepository,
                                       IAsyncRepository<SummaryByDayAndEmployee> summaryByDayAndEmployeeRepository,
                                       IAsyncRepository<SummaryByEmployees> summaryByEmployeesRepository,
                                       IAsyncRepository<SummaryByDepartments> summaryByDepartmentsRepository,
                                       IAsyncRepository<SummaryByWorkstations> summaryByWorkstationsRepository)
        {
            _workstationSessionRepository = workstationSessionRepository;
            _workstationEventRepository = workstationEventRepository;
            _workstationRepository = workstationRepository;
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _companyRepository = companyRepository;
            _departmentRepository = departmentRepository;
            _deviceAccountRepository = deviceAccountRepository;
            _summaryByDayAndEmployeeRepository = summaryByDayAndEmployeeRepository;
            _summaryByEmployeesRepository = summaryByEmployeesRepository;
            _summaryByDepartmentsRepository = summaryByDepartmentsRepository;
            _summaryByWorkstationsRepository = summaryByWorkstationsRepository;
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

        public IQueryable<SummaryByDayAndEmployee> SummaryByDayAndEmployeeSqlQuery(string sql)
        {
            return _summaryByDayAndEmployeeRepository.SqlQuery(sql);
        }

        public IQueryable<SummaryByEmployees> SummaryByEmployeesSqlQuery(string sql)
        {
            return _summaryByEmployeesRepository.SqlQuery(sql);
        }

        public IQueryable<SummaryByDepartments> SummaryByDepartmentsSqlQuery(string sql)
        {
            return _summaryByDepartmentsRepository.SqlQuery(sql);
        }

        public IQueryable<SummaryByWorkstations> SummaryByWorkstationsSqlQuery(string sql)
        {
            return _summaryByWorkstationsRepository.SqlQuery(sql);
        }

        public async Task AddSessionAsync(WorkstationSession workstationSession)
        {
            if (workstationSession == null)
            {
                throw new ArgumentNullException(nameof(workstationSession));
            }

            await _workstationSessionRepository.AddAsync(workstationSession);
        }

        public async Task UpdateWorkstationSessionsAsync(IList<WorkstationEvent> events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            foreach (var e in events)
            {
                var lastSession = await _workstationSessionRepository
                    .Query()
                    .OrderBy(o => o.EndDate)
                    .AsNoTracking()
                    .LastOrDefaultAsync(s => s.EndDate == null && s.WorkstationId == e.WorkstationId);

                if (e.EventId == WorkstationEventType.ComputerLock || e.EventId == WorkstationEventType.ComputerLogoff)
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
                        lastSession.EndDate = e.Date;
                        await _workstationSessionRepository.UpdateAsync(lastSession);
                        continue;
                    }
                }

                if (e.EventId == WorkstationEventType.ComputerLogon || e.EventId == WorkstationEventType.ComputerUnlock)
                {
                    if (lastSession != null)
                    {
                        // There is an unfinished session for current workstation
                        lastSession.EndDate = e.Date;
                        await _workstationSessionRepository.UpdateAsync(lastSession);

                        // Todo: add warning notification: created a new session while the previous one was still active
                    }

                    var newSession = CreateSessionFromEvent(e);
                    await _workstationSessionRepository.AddAsync(newSession);
                    continue;
                }
            }
        }

        private WorkstationSession CreateSessionFromEvent(WorkstationEvent workstationEvent)
        {
            Enum.TryParse(typeof(SessionSwitchSubject), workstationEvent.Note, out object unlockMethod);
            SessionSwitchSubject unlockedBy = unlockMethod == null ? SessionSwitchSubject.NonHideez : (SessionSwitchSubject)unlockMethod;

            return new WorkstationSession()
            {
                StartDate = workstationEvent.Date,
                EndDate = null,
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