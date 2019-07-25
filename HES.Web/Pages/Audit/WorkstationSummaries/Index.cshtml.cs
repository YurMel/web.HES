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
            #region linq
            //SummaryByDayAndEmployee = await _workstationSessionService
            //   .WorkstationSessionQuery()
            //   .Include(w => w.Employee)
            //   .Include(w => w.Department.Company)
            //   .GroupBy(w => new
            //   {
            //       w.StartTime.Date,
            //       w.Employee,
            //   })
            //   .Select(g => new SummaryByDayAndEmployee()
            //   {
            //       Date = g.Key.Date,
            //       //Employee = g.First().Employee ?? new Employee { Id = "N/A", FirstName = "N/A", LastName = "" },
            //       //Department = g.First().Department ?? new Department { Company = new Company { Id = "N/A", Name = "N/A" }, Id = "N/A", Name = "N/A" },
            //       WorkstationsCount = g.GroupBy(z => z.Workstation.Id).Count(),
            //       AvgSessionDuration = TimeSpan.FromMinutes(g.Average(s => s.Duration.TotalMinutes)),
            //       SessionCount = g.Count(),
            //       TotalSessionDuration = TimeSpan.FromMinutes(g.Sum(s => s.Duration.TotalMinutes)),
            //   })
            //      .OrderByDescending(w => w.Date)
            //      //.OrderBy(w => w.Employee.FullName)
            //      .Take(100)
            //      .AsNoTracking()
            //      .ToListAsync();

            //if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            //{
            //    //filter = filter
            //    //    .Where(w => w.Date >= SummaryFilter.StartDate.Value.Date.ToUniversalTime())
            //    //    .Where(w => w.Date <= SummaryFilter.EndDate.Value.Date.ToUniversalTime());
            //}
            //if (SummaryFilter.EmployeeId != null)
            //{
            //    //filter = filter.Where(w => w.Employee.Id == SummaryFilter.EmployeeId);
            //}
            //if (SummaryFilter.CompanyId != null)
            //{
            //    //filter = filter.Where(w => w.Department.Company.Id == SummaryFilter.CompanyId);
            //}
            //if (SummaryFilter.DepartmentId != null)
            //{
            //    //filter = filter.Where(w => w.Department.Id == SummaryFilter.DepartmentId);
            //}

            //SummaryByDayAndEmployee = await filter.AsNoTracking().ToListAsync();
            #endregion

            SummaryByDayAndEmployee = await _workstationSessionService
                .SummaryByDayAndEmployeeSqlQuery
                ($@"SELECT
	                    DATE(workstationsessions.StartTime) AS Date,
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS AvgSessionDuration,
	                    COUNT(*) AS SessionCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS TotalSessionDuration
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                    WHERE workstationsessions.StartTime BETWEEN '{DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss")}' AND '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}'
                    GROUP BY
	                    DATE(workstationsessions.StartTime),
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    DATE(workstationsessions.StartTime) DESC, Employee ASC")
                .AsNoTracking()
                .ToListAsync();

            ViewData["Employees"] = new SelectList(await _workstationSessionService.EmployeeQuery().ToListAsync(), "Id", "FullName");
            ViewData["Companies"] = new SelectList(await _workstationSessionService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["Departments"] = new SelectList(await _workstationSessionService.DepartmentQuery().ToListAsync(), "Id", "Name");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper();

            ViewData["StartDate"] = DateTime.UtcNow.AddDays(-30);
            ViewData["EndDate"] = DateTime.UtcNow;
        }

        public async Task<IActionResult> OnPostFilterSummaryByDaysAndEmployeesAsync(SummaryFilter SummaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartTime BETWEEN '{SummaryFilter.StartDate.Value.Date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{SummaryFilter.EndDate.Value.Date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            else
            {
                parameters.Add($"workstationsessions.StartTime BETWEEN '{DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss")}' AND '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (SummaryFilter.EmployeeId != null)
            {
                if (SummaryFilter.EmployeeId == "N/A")
                {
                    parameters.Add($"employees.Id IS NULL");
                }
                else
                {
                    parameters.Add($"employees.Id = '{SummaryFilter.EmployeeId}'");
                }
            }
            if (SummaryFilter.CompanyId != null)
            {
                if (SummaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{SummaryFilter.CompanyId}'");
                }
            }
            if (SummaryFilter.DepartmentId != null)
            {
                if (SummaryFilter.DepartmentId == "N/A")
                {
                    parameters.Add($"departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"departments.Id = '{SummaryFilter.DepartmentId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            SummaryByDayAndEmployee = await _workstationSessionService
                .SummaryByDayAndEmployeeSqlQuery
                ($@"SELECT
	                    DATE(workstationsessions.StartTime) AS Date,
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS AvgSessionDuration,
	                    COUNT(*) AS SessionCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS TotalSessionDuration
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    DATE(workstationsessions.StartTime),
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    DATE(workstationsessions.StartTime) DESC, Employee ASC")
                .AsNoTracking()
                .ToListAsync();

            return Partial("_ByDaysAndEmployees", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByEmployeesAsync(SummaryFilter SummaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartTime BETWEEN '{SummaryFilter.StartDate.Value.Date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{SummaryFilter.EndDate.Value.Date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            else
            {
                parameters.Add($"workstationsessions.StartTime BETWEEN '{DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss")}' AND '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (SummaryFilter.CompanyId != null)
            {
                if (SummaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{SummaryFilter.CompanyId}'");
                }
            }
            if (SummaryFilter.DepartmentId != null)
            {
                if (SummaryFilter.DepartmentId == "N/A")
                {
                    parameters.Add($"departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"departments.Id = '{SummaryFilter.DepartmentId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            SummaryByEmployees = await _workstationSessionService
                .SummaryByEmployeesSqlQuery
                ($@"SELECT
	                    employees.Id AS EmployeeId,
	                    IFNULL(CONCAT(employees.FirstName,' ',employees.LastName), 'N/A') AS Employee,
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(DISTINCT DATE(workstationsessions.StartTime)) AS WorkingDaysCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS AvgSessionDuration,	
	                    COUNT(*) / COUNT(DISTINCT DATE(workstationsessions.StartTime)) AS AvgSessionCountPerDay,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime))) / COUNT(DISTINCT DATE(workstationsessions.StartTime))) AS AvgWorkingHoursPerDay
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    workstationsessions.EmployeeId
                    ORDER BY
	                    Employee ASC")
                .AsNoTracking()
                .ToListAsync();

            return Partial("_ByEmployees", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByDepartmentsAsync(SummaryFilter SummaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartTime BETWEEN '{SummaryFilter.StartDate.Value.Date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{SummaryFilter.EndDate.Value.Date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            else
            {
                parameters.Add($"workstationsessions.StartTime BETWEEN '{DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss")}' AND '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (SummaryFilter.CompanyId != null)
            {
                if (SummaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{SummaryFilter.CompanyId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            SummaryByDepartments = await _workstationSessionService
                .SummaryByDepartmentsSqlQuery
                ($@"SELECT
	                    companies.Id AS CompanyId,
	                    IFNULL(companies.Name, 'N/A') AS Company,
	                    departments.Id AS DepartmentId,
	                    IFNULL(departments.Name, 'N/A') AS Department,
	                    COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(DISTINCT WorkstationId) AS WorkstationsCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS AvgSessionDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime))) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS AvgTotalSessionCountByEmployee
                    FROM workstationsessions
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    departments.Id
                    ORDER BY
	                    Company ASC, Department ASC")
                .AsNoTracking()
                .ToListAsync();

            return Partial("_ByDepartments", this);
        }

        public async Task<IActionResult> OnPostFilterSummaryByWorkstationsAsync(SummaryFilter SummaryFilter)
        {
            var where = string.Empty;
            List<string> parameters = new List<string>();

            if (SummaryFilter.StartDate != null && SummaryFilter.EndDate != null)
            {
                parameters.Add($"workstationsessions.StartTime BETWEEN '{SummaryFilter.StartDate.Value.Date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}' AND '{SummaryFilter.EndDate.Value.Date.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            else
            {
                parameters.Add($"workstationsessions.StartTime BETWEEN '{DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss")}' AND '{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}'");
            }
            if (SummaryFilter.EmployeeId != null)
            {
                if (SummaryFilter.EmployeeId == "N/A")
                {
                    parameters.Add($"employees.Id IS NULL");
                }
                else
                {
                    parameters.Add($"employees.Id = '{SummaryFilter.EmployeeId}'");
                }
            }
            if (SummaryFilter.CompanyId != null)
            {
                if (SummaryFilter.CompanyId == "N/A")
                {
                    parameters.Add($"companies.Id IS NULL");
                }
                else
                {
                    parameters.Add($"companies.Id = '{SummaryFilter.CompanyId}'");
                }
            }
            if (SummaryFilter.DepartmentId != null)
            {
                if (SummaryFilter.DepartmentId == "N/A")
                {
                    parameters.Add($"departments.Id IS NULL");
                }
                else
                {
                    parameters.Add($"departments.Id = '{SummaryFilter.DepartmentId}'");
                }
            }

            if (parameters.Count > 0)
            {
                where = string.Join(" AND ", parameters).Insert(0, "WHERE ");
            }

            SummaryByWorkstations = await _workstationSessionService
                .SummaryByWorkstationsSqlQuery
                ($@"SELECT
	                    workstations.Name AS Workstation,
	                    COUNT(DISTINCT IFNULL(companies.Id, 'N/A')) AS CompaniesCount,
	                    COUNT(DISTINCT IFNULL(departments.Id, 'N/A')) AS DepartmentsCount,
	                    COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS EmployeesCount,
	                    COUNT(*) AS TotalSessionsCount,
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS TotalSessionsDuration,	
	                    SEC_TO_TIME(AVG(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime)))) AS AvgSessionDuration,	
	                    SEC_TO_TIME(SUM(TIME_TO_SEC(TIMEDIFF(IFNULL(workstationsessions.EndTime, NOW()), workstationsessions.StartTime))) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A'))) AS AvgTotalDuartionByEmployee,
	                    COUNT(*) / COUNT(DISTINCT IFNULL(employees.Id, 'N/A')) AS AvgTotalSessionCountByEmployee
                    FROM workstationsessions
	                    LEFT JOIN workstations ON workstationsessions.WorkstationId = workstations.Id
	                    LEFT JOIN employees ON workstationsessions.EmployeeId = employees.Id
	                    LEFT JOIN departments ON employees.DepartmentId = departments.Id
	                    LEFT JOIN companies ON departments.CompanyId = companies.Id
                {where}
                    GROUP BY
	                    WorkstationId")
                .AsNoTracking()
                .ToListAsync();

            return Partial("_ByWorkstations", this);
        }
    }
}