using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DeviceAccessProfiles
{
    public class IndexModel : PageModel
    {
        private readonly IDeviceAccessProfilesService _deviceAccessProfilesService;
        private readonly ILogger<IndexModel> _logger;

        public IList<DeviceAccessProfile> DeviceAccessProfiles { get; set; }
        public DeviceAccessProfile DeviceAccessProfile { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IDeviceAccessProfilesService deviceAccessProfilesService, ILogger<IndexModel> logger)
        {
            _deviceAccessProfilesService = deviceAccessProfilesService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            DeviceAccessProfiles = await _deviceAccessProfilesService
                .DeviceAccessProfilesQuery()
                .Include(d => d.Devices)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetDetailsProfile(string id)
        {
            DeviceAccessProfile = await _deviceAccessProfilesService.GetByIdAsync(id);
            return Partial("_DetailsProfile", this);
        }
    }
}
