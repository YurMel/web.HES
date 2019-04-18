using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    [DefaultBreadcrumb("Home")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        public IList<Employee> Employees { get; set; }
        public bool HasForeignKey { get; set; }
        [BindProperty]
        public Employee Employee { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ApplicationDbContext context, IEmployeeService employeeService, IDeviceService deviceService)
        {
            _context = context;
            _employeeService = employeeService;
            _deviceService = deviceService;
        }

        public async Task OnGetAsync()
        {
            Employees = await _employeeService.GetAllIncludeAsync(e => e.Department.Company, e => e.Department, e => e.Position, e => e.Devices);
        }

        #region Employee

        public IActionResult OnGetCreateEmployee()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");

            return Partial("_CreateEmployee", this);
        }

        public async Task<IActionResult> OnPostCreateEmployeeAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                await _employeeService.CreateEmployeeAsync(Employee);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteEmployeeAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee = await _employeeService.GetFirstOrDefaulAsync(m => m.Id == id);

            if (Employee == null)
            {
                return NotFound();
            }

            HasForeignKey = _deviceService.Exist(x => x.EmployeeId == id);

            return Partial("_DeleteEmployee", this);
        }

        public async Task<IActionResult> OnPostDeleteEmployeeAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                await _employeeService.DeleteEmployeeAsync(id);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}
