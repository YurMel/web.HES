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

        public string ServerVersion { get; set; }
        public int ServerRemoteTasksCount { get; set; }
        public int RegisteredEmployeesCount { get; set; }
        public int EmployeesSessionsCount { get; set; }
        public int RegisteredDevicesCount { get; set; }
        public int FreeDevicesCount { get; set; }
        public int RegisteredWorkstationsCount { get; set; }
        public int WorkstationsOnlineCount { get; set; }

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
            try
            {
                ServerVersion = _dashboardService.GetServerVersion();
                ServerRemoteTasksCount = await _dashboardService.GetDeviceTasksCount();
                RegisteredEmployeesCount = await _dashboardService.GetEmployeesCount();
                EmployeesSessionsCount = await _dashboardService.GetEmployeesOpenedSessionsCount();
                RegisteredDevicesCount = await _dashboardService.GetDevicesCount();
                FreeDevicesCount = await _dashboardService.GetFreeDevicesCount();
                RegisteredWorkstationsCount = await _dashboardService.GetWorkstationsCount();
                WorkstationsOnlineCount = await _dashboardService.GetWorkstationsOnlineCount();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }
        }
    }
}