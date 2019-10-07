using HES.Core.Entities;
using HES.Core.Entities.Models;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class WorkstationSessionService : IWorkstationSessionService
    {
        private readonly IAsyncRepository<WorkstationSession> _workstationSessionRepository;
        private readonly IAsyncRepository<Device> _deviceRepository;
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        private readonly IAsyncRepository<SummaryByDayAndEmployee> _summaryByDayAndEmployeeRepository;
        private readonly IAsyncRepository<SummaryByEmployees> _summaryByEmployeesRepository;
        private readonly IAsyncRepository<SummaryByDepartments> _summaryByDepartmentsRepository;
        private readonly IAsyncRepository<SummaryByWorkstations> _summaryByWorkstationsRepository;
        private readonly ILogger<WorkstationSessionService> _logger;

        public WorkstationSessionService(IAsyncRepository<WorkstationSession> workstationSessionRepository,
                                         IAsyncRepository<Device> deviceRepository,
                                         IAsyncRepository<Employee> employeeRepository,
                                         IAsyncRepository<DeviceAccount> deviceAccountRepository,
                                         IAsyncRepository<SummaryByDayAndEmployee> summaryByDayAndEmployeeRepository,
                                         IAsyncRepository<SummaryByEmployees> summaryByEmployeesRepository,
                                         IAsyncRepository<SummaryByDepartments> summaryByDepartmentsRepository,
                                         IAsyncRepository<SummaryByWorkstations> summaryByWorkstationsRepository,
                                         ILogger<WorkstationSessionService> logger)
        {
            _workstationSessionRepository = workstationSessionRepository;
            _deviceRepository = deviceRepository;
            _employeeRepository = employeeRepository;
            _deviceAccountRepository = deviceAccountRepository;
            _summaryByDayAndEmployeeRepository = summaryByDayAndEmployeeRepository;
            _summaryByEmployeesRepository = summaryByEmployeesRepository;
            _summaryByDepartmentsRepository = summaryByDepartmentsRepository;
            _summaryByWorkstationsRepository = summaryByWorkstationsRepository;
            _logger = logger;
        }

        public IQueryable<WorkstationSession> Query()
        {
            return _workstationSessionRepository.Query();
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

        public async Task AddOrUpdateWorkstationSessions(IList<WorkstationEventDto> workstationEventsDto)
        {
            if (workstationEventsDto == null)
            {
                throw new Exception(nameof(workstationEventsDto));
            }

            foreach (var workstationEvent in workstationEventsDto)
            {
                // On unlock
                if ((workstationEvent.EventId == WorkstationEventType.HESConnected ||
                     workstationEvent.EventId == WorkstationEventType.ServiceStarted ||
                     workstationEvent.EventId == WorkstationEventType.ComputerUnlock ||
                     workstationEvent.EventId == WorkstationEventType.ComputerLogon) &&
                     workstationEvent.WorkstationSessionId != null)
                {
                    var session = await _workstationSessionRepository.Query()
                        .FirstOrDefaultAsync(w => w.Id == workstationEvent.WorkstationSessionId);

                    if (session == null)
                    {
                        // Add new session
                        await AddSessionAsync(workstationEvent);
                    }
                    else
                    {
                        // Reopen session
                        session.EndDate = null;
                        await UpdateSessionAsync(session);
                    }
                }

                // On disconnected or lock
                if ((workstationEvent.EventId == WorkstationEventType.ComputerLock ||
                     workstationEvent.EventId == WorkstationEventType.ComputerLogoff) &&
                     workstationEvent.WorkstationSessionId != null)
                {
                    var session = await _workstationSessionRepository.Query()
                        .FirstOrDefaultAsync(w => w.Id == workstationEvent.WorkstationSessionId);

                    if (session == null)
                    {
                        _logger.LogCritical($"[{workstationEvent.WorkstationId}] Сannot find last session for closing");
                        continue;
                    }

                    session.EndDate = workstationEvent.Date;
                    await UpdateSessionAsync(session);
                }
            }
        }

        private async Task AddSessionAsync(WorkstationEventDto workstationEventDto)
        {
            if (workstationEventDto == null)
            {
                throw new ArgumentNullException(nameof(workstationEventDto));
            }

            Enum.TryParse(typeof(SessionSwitchSubject), workstationEventDto.Note, out object unlockMethod);
            SessionSwitchSubject unlockedBy = unlockMethod == null ? SessionSwitchSubject.NonHideez : (SessionSwitchSubject)unlockMethod;

            string employeeId = null;
            string departmentId = null;
            string deviceAccountId = null;

            if (workstationEventDto.DeviceId != null)
            {
                var device = await _deviceRepository.GetByIdAsync(workstationEventDto.DeviceId);
                var employee = await _employeeRepository.GetByIdAsync(device.EmployeeId);
                var deviceAccount = await _deviceAccountRepository
                    .Query()
                    .Where(d => d.Name == workstationEventDto.AccountName && d.Login == workstationEventDto.AccountLogin && d.DeviceId == workstationEventDto.DeviceId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                employeeId = device?.EmployeeId;
                departmentId = employee?.DepartmentId;
                deviceAccountId = deviceAccount?.Id;
            }

            var workstationSession = new WorkstationSession()
            {
                Id = workstationEventDto.WorkstationSessionId,
                StartDate = workstationEventDto.Date,
                EndDate = null,
                UnlockedBy = unlockedBy,
                WorkstationId = workstationEventDto.WorkstationId,
                UserSession = workstationEventDto.UserSession,
                DeviceId = workstationEventDto.DeviceId,
                EmployeeId = employeeId,
                DepartmentId = departmentId,
                DeviceAccountId = deviceAccountId,
            };

            await _workstationSessionRepository.AddAsync(workstationSession);
        }

        private async Task UpdateSessionAsync(WorkstationSession workstationSession)
        {
            if (workstationSession == null)
            {
                throw new ArgumentNullException(nameof(workstationSession));
            }

            await _workstationSessionRepository.UpdateOnlyPropAsync(workstationSession, new string[] { "EndDate" });
        }

        public async Task CloseSessionAsync(string workstationId)
        {
            var session = await _workstationSessionRepository
                .Query()
                .Where(w => w.WorkstationId == workstationId && w.EndDate == null)
                .FirstOrDefaultAsync();

            if (session == null)
            {
                _logger.LogCritical($"[{workstationId}] Сannot find last session for closing");
                return;
            }

            session.EndDate = DateTime.UtcNow;
            await UpdateSessionAsync(session);
        }
    }
}