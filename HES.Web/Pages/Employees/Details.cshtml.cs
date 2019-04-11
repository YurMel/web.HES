using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HES.Web.Pages.Employees
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly IDeviceService _deviceService;

        public IList<Device> Devices { get; set; }
        public IList<DeviceAccount> DeviceAccounts { get; set; }

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

        public DetailsModel(ApplicationDbContext context, IDeviceAccountService deviceAccountService, IDeviceService deviceService)
        {
            _context = context;
            _deviceAccountService = deviceAccountService;
            _deviceService = deviceService;
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

        public async Task<IActionResult> OnPostEditEmployeeAsync(string id, Employee Employee)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Details", new { id });
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

        public async Task<IActionResult> OnGetCreatePersonalAccountAsync(string id)
        {
            ViewData["EmployeeId"] = id;

            Devices = await _deviceService.GetAllWhereAsync(d => d.EmployeeId == id);

            return Partial("_CreatePersonalAccount", this);
        }

        public async Task<IActionResult> OnPostCreatePersonalAccountAsync(DeviceAccount DeviceAccount, InputModel Input, string[] SelectedDevices)
        {
            var id = DeviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Details", new { id });
            }

            List<DeviceAccount> Accounts = new List<DeviceAccount>();
            List<DeviceTask> Tasks = new List<DeviceTask>();

            foreach (var device in SelectedDevices)
            {
                // Device Account
                var deviceAccount = new DeviceAccount();
                deviceAccount.Id = Guid.NewGuid().ToString();
                deviceAccount.Name = DeviceAccount.Name;
                deviceAccount.Urls = DeviceAccount.Urls;
                deviceAccount.Apps = DeviceAccount.Apps;
                deviceAccount.Login = DeviceAccount.Login;
                deviceAccount.Type = AccountType.Personal;
                deviceAccount.Status = AccountStatus.Creating;
                deviceAccount.LastSyncedAt = null;
                deviceAccount.CreatedAt = DateTime.Now;
                deviceAccount.PasswordUpdatedAt = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(Input.OtpSecret)) { deviceAccount.OtpUpdatedAt = DateTime.Now; }
                deviceAccount.Deleted = false;
                deviceAccount.EmployeeId = DeviceAccount.EmployeeId;
                deviceAccount.DeviceId = device;
                deviceAccount.SharedAccount = null;
                Accounts.Add(deviceAccount);

                // Device Task
                var deviceTask = new DeviceTask();
                deviceTask.DeviceId = device;
                deviceTask.DeviceAccountId = deviceAccount.Id;
                deviceTask.Password = Input.Password;
                deviceTask.NameChanged = true;
                deviceTask.UrlsChanged = true;
                deviceTask.AppsChanged = true;
                deviceTask.LoginChanged = true;
                deviceTask.PasswordChanged = true;
                if (!string.IsNullOrWhiteSpace(Input.OtpSecret))
                {
                    deviceTask.OtpSecret = Input.OtpSecret;
                    deviceTask.OtpSecretChanged = true;
                }
                deviceTask.CreatedAt = DateTime.Now;
                deviceTask.Operation = TaskOperation.Create;
                Tasks.Add(deviceTask);
            }

            await _deviceAccountService.AddRangeAsync(Accounts);
            await _deviceAccountService.AddRangeAsync(Tasks);

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _deviceAccountService.GetFirstOrDefaulIncludeAsync(a => a.Id == id, e => e.Employee, e => e.Device);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_EditPersonalAccount", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountAsync(DeviceAccount DeviceAccount)
        {
            var id = DeviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                // Update Device Account
                string[] properties = { "Name", "Urls", "Apps", "Login" };
                await _deviceAccountService.UpdateOnlyPropAsync(DeviceAccount, properties);

                // Create Device Task    
                var deviceTask = new DeviceTask();
                deviceTask.DeviceId = DeviceAccount.DeviceId;
                deviceTask.DeviceAccountId = DeviceAccount.Id;
                deviceTask.Password = null;
                deviceTask.OtpSecret = null;
                deviceTask.CreatedAt = DateTime.Now;
                deviceTask.Operation = TaskOperation.Update;
                deviceTask.NameChanged = true;
                deviceTask.UrlsChanged = true;
                deviceTask.AppsChanged = true;
                deviceTask.LoginChanged = true;
                await _deviceAccountService.AddAsync(deviceTask);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceAccountExists(DeviceAccount.Id))
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

        public async Task<IActionResult> OnGetEditPersonalAccountPwdAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _deviceAccountService.GetFirstOrDefaulIncludeAsync(a => a.Id == id, e => e.Employee, e => e.Device);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_EditPersonalAccountPwd", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountPwdAsync(DeviceAccount DeviceAccount, InputModel Input)
        {
            var id = DeviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                // Update Device Account
                var deviceAccount = _deviceAccountService.CreateDeviceAccount(DeviceAccount.Id, null, null, null, null, AccountType.Personal, AccountStatus.Updating, null, DeviceAccount.EmployeeId, DeviceAccount.DeviceId, null);
                string[] properties = { "Status", "PasswordUpdatedAt" };
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);

                // Create Device Task
                var deviceTask = _deviceAccountService.CreateDeviceTask(DeviceAccount.DeviceId, DeviceAccount.Id, Input.Password, null, DateTime.Now, TaskOperation.Update, false, false, false, false, true, false);
                await _deviceAccountService.AddAsync(deviceTask);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceAccountExists(DeviceAccount.Id))
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

        public async Task<IActionResult> OnGetEditPersonalAccountOtpAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _deviceAccountService.GetFirstOrDefaulIncludeAsync(a => a.Id == id, e => e.Employee, e => e.Device);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_EditPersonalAccountOtp", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountOtpAsync(DeviceAccount DeviceAccount, InputModel Input)
        {
            var id = DeviceAccount.EmployeeId;

            if (string.IsNullOrWhiteSpace(Input.OtpSecret))
            {
                Input.OtpSecret = null;
            }

            try
            {
                // Update Device Account
                var deviceAccount = _deviceAccountService.CreateDeviceAccount(DeviceAccount.Id, null, null, null, null, AccountType.Personal, AccountStatus.Updating, Input.OtpSecret, DeviceAccount.EmployeeId, DeviceAccount.DeviceId, null);
                string[] properties = { "Status", "OtpUpdatedAt" };
                await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, properties);

                // Create Device Task
                var deviceTask = _deviceAccountService.CreateDeviceTask(DeviceAccount.DeviceId, DeviceAccount.Id, null, Input.OtpSecret, DateTime.Now, TaskOperation.Update, false, false, false, false, false, true);
                await _deviceAccountService.AddAsync(deviceTask);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceAccountExists(DeviceAccount.Id))
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

        private bool DeviceAccountExists(string id)
        {
            return _deviceAccountService.Exist(e => e.Id == id);
        }

        public async Task<IActionResult> OnGetDeletePersonalAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _deviceAccountService.GetFirstOrDefaulAsync(m => m.Id == id);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_DeletePersonalAccount", this);
        }

        public async Task<IActionResult> OnPostDeletePersonalAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _deviceAccountService.GetByIdAsync(id);
            id = DeviceAccount.EmployeeId;
            if (DeviceAccount != null)
            {
                await _deviceAccountService.DeleteAsync(DeviceAccount);
            }

            return RedirectToPage("./Details", new { id });
        }

        #endregion
    }
}