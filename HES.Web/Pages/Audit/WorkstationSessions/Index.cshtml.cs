using HES.Core.Entities;
using HES.Core.Entities.Models;
using HES.Core.Interfaces;
using Hideez.SDK.Communication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSessions
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationSessionService _workstationSessionService;
        public IList<WorkstationSession> WorkstationSessions { get; set; }
        public WorkstationSessionFilter WorkstationSessionFilter { get; set; }

        public IndexModel(IWorkstationSessionService workstationSessionService)
        {
            _workstationSessionService = workstationSessionService;
        }

        public async Task OnGetAsync()
        {
            WorkstationSessions = await _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .Where(w => w.StartTime >= DateTime.UtcNow.AddDays(-30) && w.EndTime <= DateTime.UtcNow)
                .OrderByDescending(w => w.StartTime)
                .ToListAsync();

            ViewData["UnlockId"] = new SelectList(Enum.GetValues(typeof(SessionSwitchSubject)).Cast<SessionSwitchSubject>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            ViewData["Workstations"] = new SelectList(await _workstationSessionService.WorkstationQuery().ToListAsync(), "Id", "Name");
            ViewData["Devices"] = new SelectList(await _workstationSessionService.DeviceQuery().ToListAsync(), "Id", "Id");
            ViewData["Employees"] = new SelectList(await _workstationSessionService.EmployeeQuery().ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _workstationSessionService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["Departments"] = new SelectList(await _workstationSessionService.DepartmentQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccounts"] = new SelectList(await _workstationSessionService.DeviceAccountQuery().ToListAsync(), "Id", "Name");
            ViewData["DeviceAccountTypes"] = new SelectList(Enum.GetValues(typeof(AccountType)).Cast<AccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper();
            ViewData["StartDate"] = DateTime.UtcNow.AddDays(-30);
            ViewData["EndDate"] = DateTime.UtcNow;
        }

        public async Task<IActionResult> OnPostFilterWorkstationSessionsAsync(WorkstationSessionFilter WorkstationSessionFilter)
        {
            var filter = _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .AsQueryable();

            if (WorkstationSessionFilter.StartTime != null && WorkstationSessionFilter.EndTime != null)
            {
                filter = filter.Where(w => w.StartTime >= WorkstationSessionFilter.StartTime.Value.ToUniversalTime() &&
                                      w.EndTime <= WorkstationSessionFilter.EndTime.Value.ToUniversalTime());
            }
            else
            {
                filter = filter.Where(w => w.StartTime >= DateTime.UtcNow.AddDays(-30) && w.EndTime <= DateTime.UtcNow);
            }
            if (WorkstationSessionFilter.Duration != null)
            {
                filter = filter.Where(w => w.Duration == WorkstationSessionFilter.Duration);
            }
            if (WorkstationSessionFilter.UnlockId != null)
            {
                filter = filter.Where(w => w.UnlockedBy == (SessionSwitchSubject)WorkstationSessionFilter.UnlockId);
            }
            if (WorkstationSessionFilter.WorkstationId != null)
            {
                filter = filter.Where(w => w.WorkstationId == WorkstationSessionFilter.WorkstationId);
            }
            if (WorkstationSessionFilter.UserSession != null)
            {
                filter = filter.Where(w => w.UserSession.Contains(WorkstationSessionFilter.UserSession));
            }
            if (WorkstationSessionFilter.DeviceId != null)
            {
                filter = filter.Where(w => w.Device.Id == WorkstationSessionFilter.DeviceId);
            }
            if (WorkstationSessionFilter.EmployeeId != null)
            {
                filter = filter.Where(w => w.EmployeeId == WorkstationSessionFilter.EmployeeId);
            }
            if (WorkstationSessionFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == WorkstationSessionFilter.CompanyId);
            }
            if (WorkstationSessionFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == WorkstationSessionFilter.DepartmentId);
            }
            if (WorkstationSessionFilter.DeviceAccountId != null)
            {
                filter = filter.Where(w => w.DeviceAccountId == WorkstationSessionFilter.DeviceAccountId);
            }
            if (WorkstationSessionFilter.DeviceAccountTypeId != null)
            {
                filter = filter.Where(w => w.DeviceAccount.Type == (AccountType)WorkstationSessionFilter.DeviceAccountTypeId);
            }

            WorkstationSessions = await filter.OrderByDescending(w => w.StartTime).ToListAsync();

            return Partial("_WorkstationSessionsTable", this);
        }
    }
}