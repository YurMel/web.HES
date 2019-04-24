using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;

namespace HES.Web.Pages.Devices
{
    [Breadcrumb("Devices")]
    public class IndexModel : PageModel
    {
        private readonly IDeviceService _deviceService;

        public IList<Device> Device { get; set; }

        public IndexModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        public async Task OnGetAsync()
        {
            Device = await _deviceService.DeviceQuery().Include(d => d.Employee).ToListAsync();
        }

        public async Task<IActionResult> OnPostPing(string id)
        {
            id = "D0A89E6BCD8D";

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