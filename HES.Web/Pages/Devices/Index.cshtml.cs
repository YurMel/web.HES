using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
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
            Device = await _deviceService.GetAllIncludeAsync(d => d.Employee);
        }
    }
}