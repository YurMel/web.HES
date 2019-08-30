using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public class DetailsModel : PageModel
    {
        private readonly IWorkstationService _workstationService;
        private readonly IEmployeeService _employeeService;
        private readonly IDeviceService _deviceService;
        private readonly ILogger<DetailsModel> _logger;

        public IList<WorkstationBinding> WorkstationBindings { get; set; }
        public IList<Device> Devices { get; set; }
        public Workstation Workstation { get; set; }
        public WorkstationBinding WorkstationBinding { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public DetailsModel(IWorkstationService workstationService,
                            IEmployeeService employeeService,
                            IDeviceService deviceService,
                            ILogger<DetailsModel> logger)
        {
            _workstationService = workstationService;
            _employeeService = employeeService;
            _deviceService = deviceService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Workstation = await _workstationService
                .WorkstationQuery()
                .Include(c => c.Department.Company)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Workstation == null)
            {
                _logger.LogWarning("Workstation == null");
                return NotFound();
            }

            WorkstationBindings = await _workstationService
                .WorkstationBindingQuery()
                .Include(d => d.Device.Employee.Department)
                .Where(d => d.WorkstationId == id)
                .ToListAsync();

            if (WorkstationBindings == null)
            {
                _logger.LogWarning("AuthorizedDevices == null");
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetAddDeviceAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Workstation = await _workstationService
            .WorkstationQuery()
            .FirstOrDefaultAsync(m => m.Id == id);

            if (Workstation == null)
            {
                _logger.LogWarning("Workstation == null");
                return NotFound();
            }

            WorkstationBindings = await _workstationService
                .WorkstationBindingQuery()
                .Where(d => d.WorkstationId == id)
                .ToListAsync();

            var deviceQuery = _deviceService.Query().AsQueryable();

            foreach (var binding in WorkstationBindings)
            {
                deviceQuery = deviceQuery.Where(d => d.Id != binding.DeviceId);
            }

            Devices = await deviceQuery
                .Include(d => d.Employee)
                .ToListAsync();

            return Partial("_AddDevice", this);
        }

        public async Task<IActionResult> OnPostAddDeviceAsync(string workstationId, bool allowBleTap, bool allowRfid, bool allowProximity, string[] selectedDevices)
        {
            if (workstationId == null)
            {
                _logger.LogWarning("workstationId == null");
                return NotFound();
            }

            try
            {
                await _workstationService.AddBindingAsync(workstationId, allowRfid, allowBleTap, allowProximity, selectedDevices);

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
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = workstationId;
            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetEditWorkstationBindingAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            WorkstationBinding = await _workstationService
                .WorkstationBindingQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (WorkstationBinding == null)
            {
                _logger.LogWarning("WorkstationBinding == null");
                return NotFound();
            }

            return Partial("_EditWorkstationBinding", this);
        }

        public async Task<IActionResult> OnPostEditWorkstationBindingAsync(WorkstationBinding WorkstationBinding)
        {
            var id = WorkstationBinding.WorkstationId;
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Details", new { id });
            }

            try
            {
                await _workstationService.EditBindingAsync(WorkstationBinding);
                SuccessMessage = $"Workstation binding updated.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnGetDeleteWorkstationBindingAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            WorkstationBinding = await _workstationService
                .WorkstationBindingQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (WorkstationBinding == null)
            {
                _logger.LogWarning("WorkstationBinding == null");
                return NotFound();
            }

            return Partial("_DeleteWorkstationBinding", this);
        }

        public async Task<IActionResult> OnPostDeleteWorkstationBindingAsync(WorkstationBinding WorkstationBinding)
        {
            if (WorkstationBinding == null)
            {
                _logger.LogWarning("WorkstationBinding == null");
                return NotFound();
            }

            try
            {
                await _workstationService.DeleteBindingAsync(WorkstationBinding.Id);
                SuccessMessage = $"Device removed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            var id = WorkstationBinding.WorkstationId;
            return RedirectToPage("./Details", new { id });
        }
    }
}