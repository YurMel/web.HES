using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly IDeviceService _deviceService;

        //private readonly ApplicationDbContext _context;
        public IList<Device> Device { get; set; }

        public IndexModel(/*ApplicationDbContext context, */IDeviceService deviceService)
        {
            //_context = context;
            _deviceService = deviceService;

        }

        public async Task OnGetAsync()
        {
            //Device = await _context.Devices.Include(d => d.Employee).ToListAsync();
            Device = await _deviceService.GetDevices();
        }
    }
}