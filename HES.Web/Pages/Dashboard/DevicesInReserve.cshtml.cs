using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HES.Web.Pages.Dashboard
{
    public class DevicesInReserveModel : PageModel
    {
        private readonly IDeviceService _deviceService;

        public IList<Device> Devices { get; set; }

        public DevicesInReserveModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        public async Task OnGet()
        {
            Devices = await _deviceService.Query().Where(d => d.EmployeeId == null).ToListAsync();
        }
    }
}