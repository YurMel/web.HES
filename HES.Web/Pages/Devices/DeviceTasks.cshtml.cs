using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public class DeviceTasksModel : PageModel
    {
        private readonly IDeviceTaskService _deviceTaskService;

        public IList<DeviceTask> DeviceTasks { get; set; }

        public DeviceTasksModel(IDeviceTaskService deviceTaskService)
        {
            _deviceTaskService = deviceTaskService;
        }
        public async Task OnGet()
        {
            DeviceTasks = await _deviceTaskService.Query().OrderByDescending(o => o.CreatedAt).ToListAsync();

        }
    }
}