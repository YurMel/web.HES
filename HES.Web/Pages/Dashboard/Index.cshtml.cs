using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace HES.Web.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<IndexModel> _logger;

        public int DeviceTaskCount { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IDashboardService dashboardService, ILogger<IndexModel> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public async Task OnGet()
        {
            DeviceTaskCount = await _dashboardService.GetDeviceTasksCount();
        }

        public async Task OnGetLoadInfoAsync()
        {
            DeviceTaskCount = await _dashboardService.GetDeviceTasksCount();
        }
    }
}