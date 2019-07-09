using HES.Core.Entities.Models;
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
        public IList<SummaryByDayAndEmployee> SummaryByDayAndEmployee { get; set; }
        public IList<SummaryByEmployees> SummaryByEmployees { get; set; }
        public IList<SummaryByDepartments> SummaryByDepartments { get; set; }
        public IList<SummaryByWorkstations> SummaryByWorkstations { get; set; }
        public SummaryFilter SummaryFilter { get; set; }

        public IndexModel(IWorkstationSessionService workstationSessionService)
        {
            _workstationSessionService = workstationSessionService;
        }

        public async Task OnGet()
        {
            SummaryByDayAndEmployee = await _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .GroupBy(w => new
                {
                    w.StartTime.Year,
                    w.StartTime.Month,
                    w.StartTime.Day,
                    w.Employee,
                })
                .Select(g => new SummaryByDayAndEmployee()
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
                .OrderBy(w => w.Employee.FirstName)
                .Take(100)
                .ToListAsync();

            ViewData["Employees"] = new SelectList(await _workstationSessionService.EmployeeQuery().ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _workstationSessionService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["Departments"] = new SelectList(await _workstationSessionService.DepartmentQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper();
        }

        public async Task<IActionResult> OnPostFilterSummaryByDaysAndEmployeesAsync(SummaryFilter SummaryFilter)
        {
            var filter = _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Department.Company)
                .GroupBy(w => new
                {
                    w.StartTime.Year,
                    w.StartTime.Month,
                    w.StartTime.Day,
                    w.Employee.Id,
                })
                .Select(g => new SummaryByDayAndEmployee()
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
                .OrderBy(w => w.Employee.FirstName)
                .AsQueryable();

            if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            {
                filter = filter
                    .Where(w => w.Date >= SummaryFilter.StartDate.Value.Date.ToUniversalTime())
                    .Where(w => w.Date <= SummaryFilter.EndDate.Value.Date.ToUniversalTime());
            }
            if (SummaryFilter.EmployeeId != null)
            {
                filter = filter.Where(w => w.Employee.Id == SummaryFilter.EmployeeId);
            }
            if (SummaryFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == SummaryFilter.CompanyId);
            }
            if (SummaryFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.Department.Id == SummaryFilter.DepartmentId);
            }

            SummaryByDayAndEmployee = await filter.ToListAsync();

            return Partial("_ByDaysAndEmployees", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByEmployeesAsync(SummaryFilter SummaryFilter)
        {
            var filter = _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .GroupBy(w => new
                {
                    w.Employee.Id,
                })
                .Select(g => new SummaryByEmployees()
                {
                    Employee = g.First().Employee,
                    Department = g.First().Department,
                    WorkstationsCount = g.GroupBy(z => z.Workstation.Id).Count(),
                    AvgSessionDuration = TimeSpan.FromMinutes(g.Average(s => s.Duration.TotalMinutes)),
                    SessionCountPerDay = g.Count() / g.GroupBy(z => z.StartTime.Day).Count(),
                    TotalSessions = g.Count(),
                    TotalSessionDurationPerDay = TimeSpan.FromMinutes(g.Sum(s => s.Duration.TotalMinutes)) / g.GroupBy(z => z.StartTime.Day).Count()
                })
                .OrderBy(w => w.Employee.FirstName)
                .AsQueryable();

            if (SummaryFilter.EmployeeId != null)
            {
                filter = filter.Where(w => w.Employee.Id == SummaryFilter.EmployeeId);
            }
            if (SummaryFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == SummaryFilter.CompanyId);
            }
            if (SummaryFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.Department.Id == SummaryFilter.DepartmentId);
            }

            SummaryByEmployees = await filter.ToListAsync();

            return Partial("_ByEmployees", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByDepartmentsAsync(SummaryFilter SummaryFilter)
        {
            var filter = _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Department.Company)
                .GroupBy(w => new
                {
                    w.Department,
                })
                .Select(g => new SummaryByDepartments()
                {
                    Department = g.First().Department,
                    EmployeesCount = g.GroupBy(z => z.EmployeeId).Count(),
                    WorkstationsCount = g.GroupBy(z => z.Workstation.Id).Count(),
                    AvgSessionDuration = TimeSpan.FromMinutes(g.Average(s => s.Duration.TotalMinutes)),
                    SessionCountPerDay = g.Count() / g.GroupBy(z => z.StartTime.Day).Count(),
                    TotalSessions = g.Count(),
                    TotalSessionDurationPerDay = TimeSpan.FromMinutes(g.Sum(s => s.Duration.TotalMinutes)) / g.GroupBy(z => z.StartTime.Day).Count()
                })
                .OrderBy(w => w.Department.Name)
                .AsQueryable();

            if (SummaryFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == SummaryFilter.CompanyId);
            }
            if (SummaryFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.Department.Id == SummaryFilter.DepartmentId);
            }

            SummaryByDepartments = await filter.ToListAsync();

            return Partial("_ByDepartments", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByWorkstationsAsync(SummaryFilter SummaryFilter)
        {
            var filter = _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Department.Company)
                .GroupBy(w => new
                {
                    w.Workstation,
                })
                .Select(g => new SummaryByWorkstations()
                {
                    Workstation = g.First().Workstation,
                    Department = g.First().Department,
                    EmployeesCount = g.GroupBy(z => z.EmployeeId).Count(),
                    AvgSessionDuration = TimeSpan.FromMinutes(g.Average(s => s.Duration.TotalMinutes)),
                    SessionCountPerDay = g.Count() / g.GroupBy(z => z.StartTime.Day).Count(),
                    TotalSessions = g.Count(),
                    TotalSessionDurationPerDay = TimeSpan.FromMinutes(g.Sum(s => s.Duration.TotalMinutes)) / g.GroupBy(z => z.StartTime.Day).Count()
                })
                .OrderBy(w => w.Workstation.Name)
                .AsQueryable();

            if (SummaryFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == SummaryFilter.CompanyId);
            }
            if (SummaryFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.Department.Id == SummaryFilter.DepartmentId);
            }

            SummaryByWorkstations = await filter.ToListAsync();

            return Partial("_ByWorkstations", this);
        }
    }
}