using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationSessionService _workstationSessionService;
        public IList<WorkstationSession> WorkstationSessions { get; set; }
        public IList<SessionsByDayAndEmployee> SessionsByDayAndEmployee { get; set; }
        public SessionsSummaryFilter SessionsSummaryFilter { get; set; }

        public IndexModel(IWorkstationSessionService workstationSessionService)
        {
            _workstationSessionService = workstationSessionService;
        }

        public async Task OnGet()
        {
            SessionsByDayAndEmployee = await _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Department.Company)
                .GroupBy(w => new
                {
                    w.StartTime.Year,
                    w.StartTime.Month,
                    w.StartTime.Day,
                    w.Employee,
                })
                .Select(g => new SessionsByDayAndEmployee()
                {
                    Date = DateTime.Parse($"{g.Key.Year}.{g.Key.Month}.{g.Key.Day}"),
                    Employee = g.First().Employee,
                    Department = g.First().Department,
                    WorkstationsCount = g.GroupBy(z => z.Workstation.Id).Count(),
                    AvgSessionDuration = TimeSpan.FromMinutes(g.Average(s => s.Duration.TotalMinutes)),
                    SessionCount = g.Count(),
                    TotalSessionDuration = TimeSpan.FromMinutes(g.Sum(s => s.Duration.TotalMinutes)),
                })
                .OrderByDescending(w => w.Date)
                .Take(100)
                .ToListAsync();

            ViewData["Employees"] = new SelectList(await _workstationSessionService.EmployeeQuery().ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _workstationSessionService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["Departments"] = new SelectList(await _workstationSessionService.DepartmentQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper();
        }

        public async Task<IActionResult> OnPostFilterSessionsByDayAndEmployeeAsync(SessionsSummaryFilter SessionsSummaryFilter)
        {
            var filter = _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Department.Company)
                .GroupBy(w => new
                {
                    w.StartTime.Year,
                    w.StartTime.Month,
                    w.StartTime.Day,
                    w.Employee,
                })
                .Select(g => new SessionsByDayAndEmployee()
                {
                    Date = DateTime.Parse($"{g.Key.Year}.{g.Key.Month}.{g.Key.Day}"),
                    Employee = g.First().Employee,
                    Department = g.First().Department,
                    WorkstationsCount = g.GroupBy(z => z.Workstation.Id).Count(),
                    AvgSessionDuration = TimeSpan.FromMinutes(g.Average(s => s.Duration.TotalMinutes)),
                    SessionCount = g.Count(),
                    TotalSessionDuration = TimeSpan.FromMinutes(g.Sum(s => s.Duration.TotalMinutes)),
                })
                .OrderByDescending(w => w.Date)
                .AsQueryable();

            if (SessionsSummaryFilter.StartDate != null && SessionsSummaryFilter.EndDate != null)
            {
                filter = filter
                    .Where(w => w.Date >= SessionsSummaryFilter.StartDate.Value.Date.ToUniversalTime())
                    .Where(w => w.Date <= SessionsSummaryFilter.EndDate.Value.Date.ToUniversalTime());
            }
            if (SessionsSummaryFilter.EmployeeId != null)
            {
                filter = filter.Where(w => w.Employee.Id == SessionsSummaryFilter.EmployeeId);
            }
            if (SessionsSummaryFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == SessionsSummaryFilter.CompanyId);
            }
            if (SessionsSummaryFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.Department.Id == SessionsSummaryFilter.DepartmentId);
            }

            SessionsByDayAndEmployee = await filter.ToListAsync();

            return Partial("_ByDaysAndEmployees", this);
        }
    }
}