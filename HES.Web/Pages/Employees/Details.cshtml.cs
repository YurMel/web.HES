using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
//using HES.Web.Data;

namespace HES.Web.Pages.Employees
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IList<Device> Devices { get; set; }
        public IList<DeviceAccount> DeviceAccounts { get; set; }

        [BindProperty]
        public Employee Employee { get; set; }
        public DeviceAccount DeviceAccount { get; set; }
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "OTP secret")]
            public string OtpSecret { get; set; }
        }

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee = await _context.Employees
                .Include(e => e.Department.Company)
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Devices).FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
            {
                return NotFound();
            }

            DeviceAccounts = await _context.DeviceAccounts
               .Include(d => d.Device)
               .Include(d => d.Employee)
               .Include(d => d.SharedAccount).ToListAsync();

            return Page();
        }

        #region Employee

        public async Task<IActionResult> OnGetEditEmployeeAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position).FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");

            return Partial("_EditEmployee", this);
        }

        public async Task<IActionResult> OnPostEditEmployeeAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Employee.Id = id;
            _context.Attach(Employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(Employee.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Details", new { id });
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        #endregion

        #region Device

        public async Task<IActionResult> OnGetAddDeviceAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee = await _context.Employees.FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
            {
                return NotFound();
            }

            Devices = await _context.Devices.Where(d => d.EmployeeId == null).ToListAsync();

            return Partial("_AddDevice", this);
        }

        public async Task<IActionResult> OnPostAddDeviceAsync(string id, string[] SelectedDevices)
        {
            if (id == null && SelectedDevices == null)
            {
                return NotFound();
            }

            Employee = await _context.Employees.FindAsync(id);

            if (Employee != null)
            {
                foreach (var device in SelectedDevices)
                {
                    var currentDevice = new Device();
                    currentDevice.Id = device;
                    currentDevice.EmployeeId = Employee.Id;
                    _context.Entry(currentDevice).Property("EmployeeId").IsModified = true;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }

            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Personal Account

        public async Task<IActionResult> OnGetCreateAccountAsync(string id)
        {
            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            ViewData["SharedAccountId"] = new SelectList(_context.SharedAccounts, "Id", "Id");

            Devices = await _context.Devices.Where(d => d.EmployeeId == id).ToListAsync();

            return Partial("_CreateAccount", this);
        }

        public async Task<IActionResult> OnPostCreateAccountAsync(string id, DeviceAccount DeviceAccount, InputModel Input, string[] SelectedDevices)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //_context.DeviceAccounts.Add(DeviceAccount);
            //await _context.SaveChangesAsync();

            return RedirectToPage("./Details", new { id });
        }

        #endregion
    }
}
