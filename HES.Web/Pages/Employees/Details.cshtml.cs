﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public class DetailsModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<DetailsModel> _logger;

        public IList<Device> Devices { get; set; }
        public IList<DeviceAccount> DeviceAccounts { get; set; }
        public IList<SharedAccount> SharedAccounts { get; set; }

        public Device Device { get; set; }
        public Employee Employee { get; set; }
        public DeviceAccount DeviceAccount { get; set; }
        public SharedAccount SharedAccount { get; set; }
        public InputModel Input { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public DetailsModel(IEmployeeService employeeService, ILogger<DetailsModel> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Employee = await _employeeService
                .EmployeeQuery()
                .Include(e => e.Department.Company)
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Devices).ThenInclude(e => e.DeviceAccessProfile)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Employee == null)
            {
                _logger.LogWarning("Employee == null");
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

        //public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        //{
        //    return new JsonResult(await _employeeService.DepartmentQuery().Where(d => d.CompanyId == id).ToListAsync());
        //}

        //public async Task<IActionResult> OnGetEditEmployeeAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        _logger.LogWarning("id == null");
        //        return NotFound();
        //    }

        //    Employee = await _employeeService
        //        .EmployeeQuery()
        //        .Include(e => e.Department)
        //        .Include(e => e.Position)
        //        .FirstOrDefaultAsync(m => m.Id == id);

        //    if (Employee == null)
        //    {
        //        _logger.LogWarning("Employee == null");
        //        return NotFound();
        //    }

        //    ViewData["CompanyId"] = new SelectList(await _employeeService.CompanyQuery().ToListAsync(), "Id", "Name");
        //    ViewData["DepartmentId"] = new SelectList(await _employeeService.DepartmentQuery().Where(d => d.CompanyId == Employee.Department.CompanyId).ToListAsync(), "Id", "Name");
        //    ViewData["PositionId"] = new SelectList(await _employeeService.PositionQuery().ToListAsync(), "Id", "Name");

        //    return Partial("_EditEmployee", this);
        //}

        //public async Task<IActionResult> OnPostEditEmployeeAsync(Employee employee)
        //{
        //    var id = employee.Id;
        //    if (!ModelState.IsValid)
        //    {
        //        _logger.LogWarning("Model is not valid");
        //        return RedirectToPage("./Details", new { id });
        //    }

        //    try
        //    {
        //        await _employeeService.EditEmployeeAsync(employee);
        //        SuccessMessage = $"Employee updated.";
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!await EmployeeExists(employee.Id))
        //        {
        //            _logger.LogError("Employee dos not exists.");
        //            return NotFound();
        //        }
        //        else
        //        {
        //            ErrorMessage = ex.Message;
        //        }
        //        _logger.LogError(ex.Message);
        //    }

        //    return RedirectToPage("./Details", new { id });
        //}

        private async Task<bool> EmployeeExists(string id)
        {
            return await _employeeService.ExistAsync(e => e.Id == id);
        }

        public async Task<IActionResult> OnGetSetPrimaryAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .Include(e => e.Employee)
                .Include(e => e.Device)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning("DeviceAccount == null");
                return NotFound();
            }

            return Partial("_SetPrimaryAccount", this);
        }

        public async Task<IActionResult> OnPostSetPrimaryAccountAsync(string deviceId, string accountId, string employeeId)
        {
            try
            {
                await _employeeService.SetPrimaryAccount(deviceId, accountId);
                SuccessMessage = "Primary account changed and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                _logger.LogWarning("employeeId == null");
                return NotFound();
            }

            try
            {
                await _employeeService.AddDeviceAsync(employeeId, selectedDevices);

                if (selectedDevices.Length > 1)
                {
                    var devices = string.Empty;
                    foreach (var item in selectedDevices)
                    {
                        devices += item + Environment.NewLine;
                    }
                    SuccessMessage = $"Devices: {devices} added.";
                }
                else
                {
                    SuccessMessage = $"Device {selectedDevices[0]} added.";
                }
            }
            catch (Exception ex)
            {
                if (!await EmployeeExists(employeeId))
                {
                    _logger.LogError("Employee dos not exists.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex.Message);
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
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Device = await _employeeService
                .DeviceQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (Device == null)
            {
                _logger.LogWarning("Device == null");
                return NotFound();
            }

            return Partial("_DeleteDevice", this);
        }

        public async Task<IActionResult> OnPostDeleteDeviceAsync(Device device)
        {
            if (device == null)
            {
                _logger.LogWarning("device == null");
                return NotFound();
            }

            try
            {
                await _employeeService.RemoveDeviceAsync(device.EmployeeId, device.Id);
                SuccessMessage = $"Device {device.Id} deleted.";
            }
            catch (Exception ex)
            {
                if (!await EmployeeExists(device.EmployeeId))
                {
                    _logger.LogError("Employee dos not exists.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex.Message);
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
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.CreatePersonalAccountAsync(deviceAccount, input, selectedDevices);
                SuccessMessage = "Account created and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                if (!await EmployeeExists(id))
                {
                    _logger.LogError("Employee dos not exists.");
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex.Message);
                    ErrorMessage = ex.Message;
                }
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .Include(e => e.Employee)
                .Include(e => e.Device)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning("DeviceAccount == null");
                return NotFound();
            }

            return Partial("_EditPersonalAccount", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountAsync(DeviceAccount deviceAccount)
        {
            var id = deviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.EditPersonalAccountAsync(deviceAccount);
                SuccessMessage = "Account updated and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountPwdAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .Include(e => e.Employee)
                .Include(e => e.Device)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning("DeviceAccount == null");
                return NotFound();
            }

            return Partial("_EditPersonalAccountPwd", this);
        }

        public async Task<IActionResult> OnPostEditPersonalAccountPwdAsync(DeviceAccount deviceAccount, InputModel input)
        {
            var id = deviceAccount.EmployeeId;

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _employeeService.EditPersonalAccountPwdAsync(deviceAccount, input);
                SuccessMessage = "Account updated and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditPersonalAccountOtpAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .Include(e => e.Employee)
                .Include(e => e.Device)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning("DeviceAccount == null");
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
                SuccessMessage = "Account updated and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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

            if (Devices == null)
            {
                _logger.LogWarning("Devices == null");
                return NotFound();
            }

            return Partial("_AddSharedAccount", this);
        }

        public async Task<IActionResult> OnPostAddSharedAccountAsync(string employeeId, string sharedAccountId, string[] selectedDevices)
        {
            if (employeeId == null)
            {
                _logger.LogWarning("employeeId == null");
                return NotFound();
            }

            try
            {
                await _employeeService.AddSharedAccount(employeeId, sharedAccountId, selectedDevices);
                SuccessMessage = "Account added and will be recorded when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning("DeviceAccount == null");
                return NotFound();
            }

            return Partial("_DeleteAccount", this);
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync(string accountId, string employeeId)
        {
            if (accountId == null)
            {
                _logger.LogWarning("accountId == null");
                return NotFound();
            }

            try
            {
                await _employeeService.DeleteAccount(accountId);
                SuccessMessage = "Account deleting and will be deleted when the device is connected to the server.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccount = await _employeeService
                .DeviceAccountQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (DeviceAccount == null)
            {
                _logger.LogWarning("DeviceAccount == null");
                return NotFound();
            }

            return Partial("_UndoChanges", this);
        }

        public async Task<IActionResult> OnPostUndoChangesAsync(string accountId, string employeeId)
        {
            if (accountId == null)
            {
                _logger.LogWarning("accountId == null");
                return NotFound();
            }

            try
            {
                await _employeeService.UndoChanges(accountId);
                SuccessMessage = "Changes were canceled.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = employeeId;
            return RedirectToPage("./Details", new { id });
        }

        #endregion
    }
}