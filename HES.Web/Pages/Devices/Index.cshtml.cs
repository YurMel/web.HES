using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly IDeviceService _deviceService;

        public Device Device { get; set; }
        public IList<Device> Devices { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        public async Task OnGetAsync()
        {
            Devices = await _deviceService.DeviceQuery().Include(d => d.Employee).ToListAsync();
        }
        
        public async Task<IActionResult> OnGetEditDeviceRfidAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Device = await _deviceService.DeviceQuery()
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Device == null)
            {
                return NotFound();
            }

            return Partial("_EditDeviceRfid", this);
        }

        public async Task<IActionResult> OnPostEditDeviceRfidAsync(Device device)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                await _deviceService.EditDeviceRfidAsync(device);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        // TEMP
        public async Task<IActionResult> OnPostPing(string id)
        {
            //id = "D0A89E6BCD8D";
            id = "D9EB34A3F4C9";

            try
            {
                if (AppHub.IsDeviceConnectedToHost(id))
                {
                    var device = await AppHub.EstablishRemoteConnection(id, 4);

                    if (device != null)
                    {
                        var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
                        var respData = await device.Ping(pingData);

                        Debug.Assert(pingData.SequenceEqual(respData.Result));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return RedirectToPage("./Index");
        }
    }
}