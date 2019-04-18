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

        public DetailsModel(ApplicationDbContext context,
            IDeviceService deviceService,
            IDeviceAccountService deviceAccountService,
            ISharedAccountService sharedAccountService,
            ITemplateService templateService,
            IEmployeeService employeeService)
        {
            _context = context;
            _deviceService = deviceService;
            _deviceAccountService = deviceAccountService;
            _sharedAccountService = sharedAccountService;
            _templateService = templateService;
            _employeeService = employeeService;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee = await _employeeService.GetFirstOrDefaulIncludeAsync(m => m.Id == id, e => e.Department.Company, e => e.Department, e => e.Position, e => e.Devices);

            if (Employee == null)
            {
                return NotFound();
            }

            DeviceAccounts = await _deviceAccountService.GetAllWhereIncludeAsync(d => d.Deleted == false, d => d.Device, d => d.Employee, d => d.SharedAccount);

            return Page();
        }

        #region Employee

        public async Task<IActionResult> OnGetEditEmployeeAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Employee = await _employeeService.GetFirstOrDefaulIncludeAsync(m => m.Id == id, e => e.Department, e => e.Position);

            if (Employee == null)
            {
                return NotFound();
            }

            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Name");
            ViewData["PositionId"] = new SelectList(_context.Positions, "Id", "Name");

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

        #endregion

        #region Device

        public async Task<IActionResult> OnGetAddDeviceAsync(string id)
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

            Devices = await _deviceService.GetAllWhereAsync(d => d.EmployeeId == null);

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

            try
            {
                await _employeeService.RemoveDeviceAsync(device.EmployeeId, device.Id);
                // TODO: Wipe Task                
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
            return new JsonResult(await _templateService.GetByIdAsync(id));
        }

        public async Task<IActionResult> OnGetCreatePersonalAccountAsync(string id)
        {
            ViewData["EmployeeId"] = id;
            ViewData["Templates"] = new SelectList(await _templateService.GetAllAsync(), "Id", "Name");

            Devices = await _deviceService.GetAllWhereAsync(d => d.EmployeeId == id);

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

            DeviceAccount = await _deviceAccountService.GetFirstOrDefaulIncludeAsync(a => a.Id == id, e => e.Employee, e => e.Device);

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

            DeviceAccount = await _deviceAccountService.GetFirstOrDefaulIncludeAsync(a => a.Id == id, e => e.Employee, e => e.Device);

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

            DeviceAccount = await _deviceAccountService.GetFirstOrDefaulIncludeAsync(a => a.Id == id, e => e.Employee, e => e.Device);

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
            if (employeeId == null)
            {
                return NotFound();
            }

            await _employeeService.AddSharedAccount(employeeId, sharedAccountId, selectedDevices);

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
                await _employeeService.DeleteAccount(accountId);
                // TODO: delete 
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