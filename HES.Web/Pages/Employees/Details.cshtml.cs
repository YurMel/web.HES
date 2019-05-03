using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public class DetailsModel : PageModel
    {
        private readonly IEmployeeService _employeeService;

        public IList<Device> Devices { get; set; }
        public IList<DeviceAccount> DeviceAccounts { get; set; }
        public IList<SharedAccount> SharedAccounts { get; set; }

        public Device Device { get; set; }
        public Employee Employee { get; set; }
        public DeviceAccount DeviceAccount { get; set; }
        public SharedAccount SharedAccount { get; set; }
        public InputModel Input { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public DetailsModel(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee = await _employeeService
                .EmployeeQuery()
                .Include(e => e.Department.Company)
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Devices)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Employee == null)
            {
                return NotFound();
            }

            DeviceAccounts = await _employeeService.DeviceAccountQuery()
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

            Employee = await _employeeService
                .EmployeeQuery()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
            {
                return NotFound();
            }

            ViewData["DepartmentId"] = new SelectList(await _employeeService.DepartmentQuery().ToListAsync(), "Id", "Name");
            ViewData["PositionId"] = new SelectList(await _employeeService.PositionQuery().ToListAsync(), "Id", "Name");

            return Partial("_EditEmployee", this);
        }

        public async Task<IActionResult> OnPostEditEmployeeAsync(Employee employee)
        {
            var id = employee.Id;
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.EditEmployeeAsync(employee);
            }
            catch (Exception ex)
            {
                if (!EmployeeExists(employee.Id))
                {
                    return NotFound();
                }
                else
                {
                    ErrorMessage = ex.Message;
                }
            }

            return RedirectToPage("./Details", new { id });
        }

        private bool EmployeeExists(string id)
        {
            return _employeeService.Exist(e => e.Id == id);
        }

        public async Task<IActionResult> OnGetSetPrimaryAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .Include(e => e.Employee)
                .Include(e => e.Device)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_SetPrimaryAccount", this);
        }

        public async Task<IActionResult> OnPostSetPrimaryAccountAsync(string deviceId, string accountId, string employeeId)
        {
            try
            {
                await _employeeService.SetPrimaryAccount(deviceId, accountId);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Device

        public async Task<IActionResult> OnGetAddDeviceAsync(string id)
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

            Devices = await _employeeService
                .DeviceQuery()
                .Where(d => d.EmployeeId == null)
                .ToListAsync();

            return Partial("_AddDevice", this);
        }

        public async Task<IActionResult> OnPostAddDeviceAsync(string employeeId, string[] selectedDevices)
        {
            if (employeeId == null)
            {
                return NotFound();
            }

            try
            {
                await _employeeService.AddDeviceAsync(employeeId, selectedDevices);
            }
            catch (Exception ex)
            {
                if (!EmployeeExists(employeeId))
                {
                    return NotFound();
                }
                else
                {
                    ErrorMessage = ex.Message;
                }
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetDeleteDeviceAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Device = await _employeeService
                .DeviceQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

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

            try
            {
                await _employeeService.RemoveDeviceAsync(device.EmployeeId, device.Id);
            }
            catch (Exception ex)
            {
                if (!EmployeeExists(device.EmployeeId))
                {
                    return NotFound();
                }
                else
                {
                    ErrorMessage = ex.Message;
                }
            }

            var id = device.EmployeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Personal Account

        public async Task<JsonResult> OnGetJsonTemplateAsync(string id)
        {
            return new JsonResult(await _employeeService.TemplateGetByIdAsync(id));
        }

        public async Task<IActionResult> OnGetCreatePersonalAccountAsync(string id)
        {
            ViewData["EmployeeId"] = id;
            ViewData["Templates"] = new SelectList(await _employeeService.TemplateQuery().ToListAsync(), "Id", "Name");

            Devices = await _employeeService.DeviceQuery().Where(d => d.EmployeeId == id).ToListAsync();

            return Partial("_CreatePersonalAccount", this);
        }

        public async Task<IActionResult> OnPostCreatePersonalAccountAsync(DeviceAccount deviceAccount, InputModel input, string[] selectedDevices)
        {
            var id = deviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.CreatePersonalAccountAsync(deviceAccount, input, selectedDevices);
            }
            catch (Exception ex)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    ErrorMessage = ex.Message;
                }
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .Include(e => e.Employee)
                .Include(e => e.Device)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_EditPersonalAccount", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountAsync(DeviceAccount deviceAccount)
        {
            var id = deviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.EditPersonalAccountAsync(deviceAccount);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountPwdAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .Include(e => e.Employee)
                .Include(e => e.Device)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_EditPersonalAccountPwd", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountPwdAsync(DeviceAccount deviceAccount, InputModel input)
        {
            var id = deviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.EditPersonalAccountPwdAsync(deviceAccount, input);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountOtpAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .Include(e => e.Employee)
                .Include(e => e.Device)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_EditPersonalAccountOtp", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountOtpAsync(DeviceAccount deviceAccount, InputModel input)
        {
            var id = deviceAccount.EmployeeId;

            try
            {
                await _employeeService.EditPersonalAccountOtpAsync(deviceAccount, input);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Shared Account

        public async Task<JsonResult> OnGetJsonSharedAccountAsync(string id)
        {
            return new JsonResult(await _employeeService.SharedAccountGetByIdAsync(id));
        }

        public async Task<IActionResult> OnGetAddSharedAccountAsync(string id)
        {
            ViewData["EmployeeId"] = id;
            ViewData["SharedAccountId"] = new SelectList(await _employeeService.SharedAccountQuery().Where(d => d.Deleted == false).ToListAsync(), "Id", "Name");

            SharedAccount = await _employeeService.SharedAccountQuery().FirstOrDefaultAsync(d => d.Deleted == false);
            Devices = await _employeeService
                .DeviceQuery()
                .Where(d => d.EmployeeId == id)
                .ToListAsync();

            return Partial("_AddSharedAccount", this);
        }

        public async Task<IActionResult> OnPostAddSharedAccountAsync(string employeeId, string sharedAccountId, string[] selectedDevices)
        {
            if (employeeId == null)
            {
                return NotFound();
            }

            try
            {
                await _employeeService.AddSharedAccount(employeeId, sharedAccountId, selectedDevices);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            var id = employeeId;
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

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

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
                await _employeeService.DeleteAccount(accountId);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion

        #region Undo

        public async Task<IActionResult> OnGetUndoChangesAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (DeviceAccount == null)
            {
                return NotFound();
            }

            return Partial("_UndoChanges", this);
        }

        public async Task<IActionResult> OnPostUndoChangesAsync(string accountId, string employeeId)
        {
            if (accountId == null)
            {
                return NotFound();
            }

            try
            {
                await _employeeService.UndoChanges(accountId);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion
    }
}