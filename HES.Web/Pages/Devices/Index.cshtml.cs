using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly IDeviceService _deviceService;
        private readonly ILogger<IndexModel> _logger;

        public Device Device { get; set; }
        public IList<Device> Devices { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IDeviceService deviceService, ILogger<IndexModel> logger)
        {
            _deviceService = deviceService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Devices = await _deviceService.DeviceQuery().Include(d => d.Employee).ToListAsync();
        }

        public async Task<IActionResult> OnGetEditDeviceRfidAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Device = await _deviceService.DeviceQuery()
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Device == null)
            {
                _logger.LogWarning("Device == null");
                return NotFound();
            }

            return Partial("_EditDeviceRfid", this);
        }

        public async Task<IActionResult> OnPostEditDeviceRfidAsync(Device device)
        {
            try
            {
                await _deviceService.EditDeviceRfidAsync(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }        
    }
}