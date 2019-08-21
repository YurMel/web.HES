﻿using HES.Core.Entities;
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
        private readonly ILogger<IndexModel> _logger;

        public IList<Device> Devices { get; set; }
        public IList<Workstation> Workstations { get; set; }
        public IList<Employee> Employees { get; set; }
        public Employee Employee { get; set; }
        public EmployeeFilter EmployeeFilter { get; set; }
        public bool HasForeignKey { get; set; }       

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IEmployeeService employeeService, IWorkstationService workstationService, ILogger<IndexModel> logger)
        {
            _employeeService = employeeService;
            _workstationService = workstationService;
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

            ViewData["Companies"] = new SelectList(await _employeeService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["Positions"] = new SelectList(await _employeeService.PositionQuery().ToListAsync(), "Id", "Name");
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

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _employeeService.DepartmentQuery().Where(d => d.CompanyId == id).ToListAsync());
        }

        public async Task<IActionResult> OnGetCreateEmployee()
        {
            ViewData["CompanyId"] = new SelectList(await _employeeService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["PositionId"] = new SelectList(await _employeeService.PositionQuery().ToListAsync(), "Id", "Name");

            Devices = await _employeeService
               .DeviceQuery()
               .Where(d => d.EmployeeId == null)
               .ToListAsync();

            Workstations = await _workstationService
                .WorkstationQuery()
                .ToListAsync();

            return Partial("_CreateEmployee", this);
        }

        public async Task<IActionResult> OnPostCreateEmployeeAsync(Employee Employee, List<string> devices, List<string> workstations)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            try
            {
                await _employeeService.CreateEmployeeAsync(Employee);
                SuccessMessage = $"Employee created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
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

        #endregion

        #region Device

        //public async Task<IActionResult> OnPostAddDeviceAsync(string employeeId, string[] selectedDevices)
        //{
        //    if (employeeId == null)
        //    {
        //        _logger.LogWarning("employeeId == null");
        //        return NotFound();
        //    }

        //    try
        //    {
        //        await _employeeService.AddDeviceAsync(employeeId, selectedDevices);

        //        if (selectedDevices.Length > 1)
        //        {
        //            var devices = string.Empty;
        //            foreach (var item in selectedDevices)
        //            {
        //                devices += item + Environment.NewLine;
        //            }
        //            SuccessMessage = $"Devices: {devices} added.";
        //        }
        //        else
        //        {
        //            SuccessMessage = $"Device {selectedDevices[0]} added.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (!await EmployeeExists(employeeId))
        //        {
        //            _logger.LogError("Employee dos not exists.");
        //            return NotFound();
        //        }
        //        else
        //        {
        //            _logger.LogError(ex.Message);
        //            ErrorMessage = ex.Message;
        //        }
        //    }

        //    var id = employeeId;
        //    return RedirectToPage("./Details", new { id });
        //}
              
        #endregion
    }
}