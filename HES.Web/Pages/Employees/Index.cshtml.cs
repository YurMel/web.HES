using HES.Core.Entities;
using HES.Core.Entities.Models;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IWorkstationService _workstationService;
        private readonly IWorkstationProximityDeviceService _workstationProximityDeviceService;
        private readonly IOrgStructureService _settingsService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Employee> Employees { get; set; }
        public IList<Device> Devices { get; set; }
        public IList<Workstation> Workstations { get; set; }
        public Employee Employee { get; set; }
        public EmployeeFilter EmployeeFilter { get; set; }
        public EmployeeWizard EmployeeWizard { get; set; }
        public Company Company { get; set; }
        public Department Department { get; set; }
        public Position Position { get; set; }

        public bool HasForeignKey { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IEmployeeService employeeService,
                          IWorkstationService workstationService,
                          IWorkstationProximityDeviceService workstationProximityDeviceService,
                          IOrgStructureService settingsService,
                          ILogger<IndexModel> logger)
        {
            _employeeService = employeeService;
            _workstationService = workstationService;
            _workstationProximityDeviceService = workstationProximityDeviceService;
            _settingsService = settingsService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Employees = await _employeeService
                .EmployeeQuery()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.Devices)
                .ToListAsync();

            ViewData["Companies"] = new SelectList(await _employeeService.CompanyQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["Positions"] = new SelectList(await _employeeService.PositionQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["DevicesCount"] = new SelectList(Employees.Select(s => s.Devices.Count()).Distinct().OrderBy(f => f).ToDictionary(t => t, t => t), "Key", "Value");

            ViewData["DatePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToLower();
            ViewData["TimePattern"] = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.ToUpper() == "H:MM" ? "hh:ii" : "hh:ii aa";
        }

        public async Task<IActionResult> OnPostFilterEmployeesAsync(EmployeeFilter EmployeeFilter)
        {
            var filter = _employeeService
                .EmployeeQuery()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.Devices)
                .AsQueryable();

            if (EmployeeFilter.CompanyId != null)
            {
                filter = filter.Where(w => w.Department.Company.Id == EmployeeFilter.CompanyId);
            }
            if (EmployeeFilter.DepartmentId != null)
            {
                filter = filter.Where(w => w.DepartmentId == EmployeeFilter.DepartmentId);
            }
            if (EmployeeFilter.PositionId != null)
            {
                filter = filter.Where(w => w.PositionId == EmployeeFilter.PositionId);
            }
            if (EmployeeFilter.DevicesCount != null)
            {
                filter = filter.Where(w => w.Devices.Count() == EmployeeFilter.DevicesCount);
            }
            if (EmployeeFilter.StartDate != null && EmployeeFilter.EndDate != null)
            {
                filter = filter.Where(w => w.LastSeen.HasValue
                                        && w.LastSeen.Value >= EmployeeFilter.StartDate.Value.AddSeconds(0).AddMilliseconds(0).ToUniversalTime()
                                        && w.LastSeen.Value <= EmployeeFilter.EndDate.Value.AddSeconds(59).AddMilliseconds(999).ToUniversalTime());
            }

            Employees = await filter
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Take(EmployeeFilter.Records)
                .ToListAsync();

            return Partial("_EmployeesTable", this);
        }

        #region Employee

        public async Task<IActionResult> OnGetCreateEmployee()
        {
            ViewData["CompanyId"] = new SelectList(await _employeeService.CompanyQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["PositionId"] = new SelectList(await _employeeService.PositionQuery().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["DeviceId"] = new SelectList(await _employeeService.DeviceQuery().Where(d => d.EmployeeId == null).ToListAsync(), "Id", "Id");
            ViewData["WorkstationId"] = new SelectList(await _workstationService.WorkstationQuery().ToListAsync(), "Id", "Name");
            ViewData["WorkstationAccountType"] = new SelectList(Enum.GetValues(typeof(WorkstationAccountType)).Cast<WorkstationAccountType>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");


            Devices = await _employeeService
               .DeviceQuery()
               .Where(d => d.EmployeeId == null)
               .ToListAsync();

            Workstations = await _workstationService
                .WorkstationQuery()
                .ToListAsync();

            return Partial("_CreateEmployee", this);
        }

        public async Task<IActionResult> OnPostCreateEmployeeAsync(EmployeeWizard EmployeeWizard)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            try
            {
                // Create employee
                var user = await _employeeService.CreateEmployeeAsync(EmployeeWizard.Employee);

                // Add device
                await _employeeService.AddDeviceAsync(user.Id, new string[] { EmployeeWizard.DeviceId });

                // Proximity
                if (EmployeeWizard.ProximityUnlock == true)
                {
                    await _workstationProximityDeviceService.AddProximityDeviceAsync(EmployeeWizard.WorkstationId, new string[] { EmployeeWizard.DeviceId });
                }

                // Add account
                await _employeeService.CreateWorkstationAccountAsync(EmployeeWizard.WorkstationAccount, user.Id, EmployeeWizard.DeviceId);

                SuccessMessage = $"Employee created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditEmployeeAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Employee = await _employeeService
                .EmployeeQuery()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
            {
                _logger.LogWarning("Employee == null");
                return NotFound();
            }

            ViewData["CompanyId"] = new SelectList(await _employeeService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(await _employeeService.DepartmentQuery().Where(d => d.CompanyId == Employee.Department.CompanyId).ToListAsync(), "Id", "Name");
            ViewData["PositionId"] = new SelectList(await _employeeService.PositionQuery().ToListAsync(), "Id", "Name");

            return Partial("_EditEmployee", this);
        }

        public async Task<IActionResult> OnPostEditEmployeeAsync(Employee employee)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            try
            {
                await _employeeService.EditEmployeeAsync(employee);
                SuccessMessage = $"Employee updated.";
            }
            catch (Exception ex)
            {
                if (!await EmployeeExistsAsync(employee.Id))
                {
                    _logger.LogError("Employee dos not exists.");
                    return NotFound();
                }
                else
                {
                    ErrorMessage = ex.Message;
                }
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteEmployeeAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Employee = await _employeeService
                .EmployeeQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
            {
                _logger.LogWarning("Employee == null");
                return NotFound();
            }

            HasForeignKey = _employeeService
                .DeviceQuery()
                .Where(x => x.EmployeeId == id)
                .Any();

            return Partial("_DeleteEmployee", this);
        }

        public async Task<IActionResult> OnPostDeleteEmployeeAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
                SuccessMessage = $"Employee deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        private async Task<bool> EmployeeExistsAsync(string id)
        {
            return await _employeeService.ExistAsync(e => e.Id == id);
        }

        #endregion

        #region Company

        public IActionResult OnGetCreateCompany()
        {
            return Partial("_CreateCompany", this);
        }

        public async Task<IActionResult> OnPostCreateCompanyAsync(Company company)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return new ContentResult() { Content = "error" };
            }

            try
            {
                await _settingsService.CreateCompanyAsync(company);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return new ContentResult() { Content = company.Name };
        }

        public async Task<JsonResult> OnGetJsonCompanyAsync()
        {
            return new JsonResult(await _employeeService.CompanyQuery().OrderBy(c => c.Name).ToListAsync());
        }

        #endregion

        #region Department

        public IActionResult OnGetCreateDepartment(string id)
        {
            ViewData["CompanyId"] = id;
            return Partial("_CreateDepartment", this);
        }

        public async Task<IActionResult> OnPostCreateDepartmentAsync(Department department)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return new ContentResult() { Content = "error" };
            }

            try
            {
                await _settingsService.CreateDepartmentAsync(department);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return new JsonResult(new { department = department.Name, company = department.CompanyId });
        }

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _employeeService.DepartmentQuery().Where(d => d.CompanyId == id).OrderBy(d => d.Name).ToListAsync());
        }

        #endregion

        #region Position

        public IActionResult OnGetCreatePosition()
        {
            return Partial("_CreatePosition", this);
        }

        public async Task<IActionResult> OnPostCreatePositionAsync(Position position)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return new ContentResult() { Content = "error" };
            }

            try
            {
                await _settingsService.CreatePositionAsync(position);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return new ContentResult() { Content = position.Name };
        }

        public async Task<JsonResult> OnGetJsonPositionAsync()
        {
            return new JsonResult(await _employeeService.PositionQuery().OrderBy(c => c.Name).ToListAsync());
        }

        #endregion
    }
}