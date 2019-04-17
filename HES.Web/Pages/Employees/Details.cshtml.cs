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
using Newtonsoft.Json;
using SmartBreadcrumbs.Attributes;

namespace HES.Web.Pages.Employees
{
    [Breadcrumb("Details", FromPage = typeof(Employees.IndexModel))]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IDeviceService _deviceService;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly ISharedAccountService _sharedAccountService;
        private readonly ITemplateService _templateService;

        public IList<Device> Devices { get; set; }
        public IList<DeviceAccount> DeviceAccounts { get; set; }
        public IList<SharedAccount> SharedAccounts { get; set; }

        public Device Device { get; set; }
        public Employee Employee { get; set; }
        public DeviceAccount DeviceAccount { get; set; }
        public SharedAccount SharedAccount { get; set; }

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

        public DetailsModel(ApplicationDbContext context, IDeviceService deviceService, IDeviceAccountService deviceAccountService, ISharedAccountService sharedAccountService, ITemplateService templateService)
        {
            _context = context;
            _deviceService = deviceService;
            _deviceAccountService = deviceAccountService;
            _sharedAccountService = sharedAccountService;
            _templateService = templateService;
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
                .Include(e => e.Devices)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
            {
                return NotFound();
            }

            DeviceAccounts = await _context.DeviceAccounts
               .Include(d => d.Device)
               .Include(d => d.Employee)
               .Include(d => d.SharedAccount)
               .Where(d => d.Deleted == false)
               .ToListAsync();

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

        public async Task<IActionResult> OnPostAddDeviceAsync(string id, string[] selectedDevices)
        {
            if (id == null && selectedDevices == null)
            {
                return NotFound();
            }

            Employee = await _context.Employees.FindAsync(id);

            if (Employee != null)
            {
                foreach (var deviceId in selectedDevices)
                {
                    var currentDevice = new Device
                    {
                        Id = deviceId,
                        EmployeeId = Employee.Id
                    };
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

        public async Task<IActionResult> OnGetDeleteDeviceAsync(string id)
        {

            if (id == null)
            {
                return NotFound();
            }

            Device = await _deviceService.GetFirstOrDefaulAsync(x => x.Id == id);

            if (Device == null)
            {
                return NotFound();
            }

            return Partial("_DeleteDevice", this);
        }

        public async Task<IActionResult> OnPostDeleteDeviceAsync(Device device)
        {
            if (device == null)
            {
                return NotFound();
            }

            var id = device.EmployeeId;

            try
            {
                // Remove employee from device
                device.EmployeeId = null;
                await _deviceService.UpdateOnlyPropAsync(device, new string[] { "EmployeeId" });
               
                // TODO: Wipe Task                
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceAccountExists(device.Id))
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

        #endregion

        #region Personal Account

        public async Task<JsonResult> OnGetJsonTemplateAsync(string id)
        {
            return new JsonResult(await _templateService.GetByIdAsync(id));
        }

        public async Task<IActionResult> OnGetCreatePersonalAccountAsync(string id)
        {
            ViewData["EmployeeId"] = id;
            ViewData["Templates"] = new SelectList(await _templateService.GetAllAsync(), "Id", "Name");

            Devices = await _deviceService.GetAllWhereAsync(d => d.EmployeeId == id);

            return Partial("_CreatePersonalAccount", this);
        }

        public async Task<IActionResult> OnPostCreatePersonalAccountAsync(DeviceAccount DeviceAccount, InputModel Input, string[] selectedDevices)
        {
            var id = DeviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Details", new { id });
            }

            List<DeviceAccount> Accounts = new List<DeviceAccount>();
            List<DeviceTask> Tasks = new List<DeviceTask>();

            foreach (var deviceId in selectedDevices)
            {
                // Device Account id
                var deviceAccountId = Guid.NewGuid().ToString();
                // Create Device Account
                Accounts.Add(new DeviceAccount { Id = deviceAccountId, Name = DeviceAccount.Name, Urls = DeviceAccount.Urls, Apps = DeviceAccount.Apps, Login = DeviceAccount.Login, Type = AccountType.Personal, Status = AccountStatus.Creating, CreatedAt = DateTime.UtcNow, PasswordUpdatedAt = DateTime.UtcNow, OtpUpdatedAt = Input.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null, EmployeeId = DeviceAccount.EmployeeId, DeviceId = deviceId, SharedAccountId = null });
                // Create Device Task - add
                Tasks.Add(new DeviceTask { DeviceAccountId = deviceAccountId, Password = Input.Password, OtpSecret = Input.OtpSecret, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Create, NameChanged = true, UrlsChanged = true, AppsChanged = true, LoginChanged = true, PasswordChanged = true, OtpSecretChanged = true });
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
                DeviceAccount.UpdatedAt = DateTime.UtcNow;
                DeviceAccount.Status = AccountStatus.Updating;
                string[] properties = { "Name", "Urls", "Apps", "Login", "Status", "UpdatedAt" };
                await _deviceAccountService.UpdateOnlyPropAsync(DeviceAccount, properties);
                // Create Device Task - update    
                await _deviceAccountService.AddAsync(new DeviceTask { DeviceAccountId = DeviceAccount.Id, Password = null, OtpSecret = null, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Update, NameChanged = true, UrlsChanged = true, AppsChanged = true, LoginChanged = true, PasswordChanged = false, OtpSecretChanged = false });
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
                string[] properties = { "Status", "UpdatedAt", "PasswordUpdatedAt" };
                await _deviceAccountService.UpdateOnlyPropAsync(new DeviceAccount { Id = DeviceAccount.Id, UpdatedAt = DateTime.UtcNow, PasswordUpdatedAt = DateTime.UtcNow, Status = AccountStatus.Updating, EmployeeId = DeviceAccount.EmployeeId, DeviceId = DeviceAccount.DeviceId, SharedAccountId = null }, properties);
                // Create Device Task - update
                await _deviceAccountService.AddAsync(new DeviceTask { DeviceAccountId = DeviceAccount.Id, Password = Input.Password, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Update, PasswordChanged = true });
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
                string[] properties = { "Status", "UpdatedAt", "OtpUpdatedAt" };
                await _deviceAccountService.UpdateOnlyPropAsync(new DeviceAccount { Id = DeviceAccount.Id, UpdatedAt = DateTime.UtcNow, OtpUpdatedAt = Input.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null, Status = AccountStatus.Updating, EmployeeId = DeviceAccount.EmployeeId, DeviceId = DeviceAccount.DeviceId, SharedAccountId = null }, properties);
                // Create Device Task - update
                await _deviceAccountService.AddAsync(new DeviceTask { DeviceAccountId = DeviceAccount.Id, OtpSecret = Input.OtpSecret, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Update, OtpSecretChanged = true });
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

        //public async Task<IActionResult> OnGetDeletePersonalAccountAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    DeviceAccount = await _deviceAccountService.FirstOrDefaulAsync(m => m.Id == id);

        //    if (DeviceAccount == null)
        //    {
        //        return NotFound();
        //    }

        //    return Partial("_DeletePersonalAccount", this);
        //}

        //public async Task<IActionResult> OnPostDeletePersonalAccountAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    DeviceAccount = await _deviceAccountService.GetByIdAsync(id);
        //    id = DeviceAccount.EmployeeId;
        //    if (DeviceAccount != null)
        //    {
        //        await _deviceAccountService.DeleteAsync(DeviceAccount);
        //    }

        //    return RedirectToPage("./Details", new { id });
        //}

        #endregion

        #region Shared Account

        public async Task<JsonResult> OnGetJsonSharedAccountAsync(string id)
        {
            return new JsonResult(await _sharedAccountService.GetByIdAsync(id));
        }

        public async Task<IActionResult> OnGetAddSharedAccountAsync(string id)
        {
            ViewData["EmployeeId"] = id;
            ViewData["SharedAccountId"] = new SelectList(await _sharedAccountService.GetAllWhereAsync(d => d.Deleted == false), "Id", "Name");

            SharedAccount = await _sharedAccountService.GetFirstOrDefaulAsync(d => d.Deleted == false);
            Devices = await _deviceService.GetAllWhereAsync(d => d.EmployeeId == id);

            return Partial("_AddSharedAccount", this);
        }

        public async Task<IActionResult> OnPostAddSharedAccountAsync(string employeeId, string sharedAccountId, string[] selectedDevices)
        {
            var id = employeeId;

            if (employeeId == null && sharedAccountId == null)
            {
                return RedirectToPage("./Details", new { id });
            }

            List<DeviceAccount> Accounts = new List<DeviceAccount>();
            List<DeviceTask> Tasks = new List<DeviceTask>();

            foreach (var deviceId in selectedDevices)
            {
                // Get Shared Account
                var sharedAccount = await _sharedAccountService.GetByIdAsync(sharedAccountId);
                // Create Device Account
                var deviceAccountId = Guid.NewGuid().ToString();
                Accounts.Add(new DeviceAccount { Id = deviceAccountId, Name = sharedAccount.Name, Urls = sharedAccount.Urls, Apps = sharedAccount.Apps, Login = sharedAccount.Login, Type = AccountType.Shared, Status = AccountStatus.Creating, CreatedAt = DateTime.UtcNow, PasswordUpdatedAt = DateTime.UtcNow, OtpUpdatedAt = sharedAccount.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null, EmployeeId = employeeId, DeviceId = deviceId, SharedAccountId = sharedAccountId });
                // Create Device Task - add
                Tasks.Add(new DeviceTask { DeviceAccountId = deviceAccountId, Password = sharedAccount.Password, OtpSecret = sharedAccount.OtpSecret, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Create, NameChanged = true, UrlsChanged = true, AppsChanged = true, LoginChanged = true, PasswordChanged = true, OtpSecretChanged = true });
            }

            await _deviceAccountService.AddRangeAsync(Accounts);
            await _deviceAccountService.AddRangeAsync(Tasks);

            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Delete Account

        public async Task<IActionResult> OnGetDeleteAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _deviceAccountService.GetFirstOrDefaulAsync(x => x.Id == id);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_DeleteAccount", this);
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync(string accountId, string employeeId)
        {
            if (accountId == null)
            {
                return NotFound();
            }

            try
            {
                // Set "Removing" status
                string[] properties = { "Status" };
                await _deviceAccountService.UpdateOnlyPropAsync(new DeviceAccount { Id = accountId, Status = AccountStatus.Removing }, properties);

                // TODO: delete acc after removing on device
                //await _deviceAccountService.UpdateOnlyPropAsync(new DeviceAccount() { Id = accountId, Deleted = true }, new string[] { "Deleted" });

                // Create Device Task - delete    
                await _deviceAccountService.AddAsync(new DeviceTask { DeviceAccountId = accountId, CreatedAt = DateTime.UtcNow, Operation = TaskOperation.Delete });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceAccountExists(accountId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion
    }
}