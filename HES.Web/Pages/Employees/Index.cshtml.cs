using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    [DefaultBreadcrumb("Home")]
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public IList<Employee> Employees { get; set; }
        public bool HasForeignKey { get; set; }

        [BindProperty]
        public Employee Employee { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task OnGetAsync()
        {
            Employees = await _employeeService
                .EmployeeQuery()
                .Include(e => e.Department.Company)
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Devices)
                .ToListAsync();
        }

        #region Employee

        public async Task<IActionResult> OnGetCreateEmployee()
        {
            ViewData["DepartmentId"] = new SelectList(await _employeeService.DepartmentQuery().ToListAsync(), "Id", "Name");
            ViewData["PositionId"] = new SelectList(await _employeeService.PositionQuery().ToListAsync(), "Id", "Name");

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

            Employee = await _employeeService
                .EmployeeQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
            {
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