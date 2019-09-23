using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HES.Web.Pages.Dashboard
{
    public class DeviceTasksModel : PageModel
    {
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly ILogger<DeviceTasksModel> _logger;

        public IList<DeviceTask> DeviceTasks { get; set; }


        public DeviceTasksModel(IDeviceTaskService deviceTaskService, ILogger<DeviceTasksModel> logger)
        {
            _deviceTaskService = deviceTaskService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            DeviceTasks = await _deviceTaskService.Query().ToListAsync();
        }
    }
}