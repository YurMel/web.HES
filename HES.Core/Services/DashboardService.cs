using HES.Core.Entities;
using HES.Core.Entities.Models;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IEmployeeService _employeeService;
        private readonly IWorkstationSessionService _workstationSessionService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IWorkstationService _workstationService;
        private readonly IDeviceService _deviceService;
        private readonly IAppService _appService;

        public DashboardService(IEmployeeService employeeService,
                                IWorkstationSessionService workstationSessionService,
                                IDeviceTaskService deviceTaskService,
                                IWorkstationService workstationService,
                                IDeviceService deviceService,
                                IAppService appService)
        {
            _employeeService = employeeService;
            _workstationSessionService = workstationSessionService;
            _deviceTaskService = deviceTaskService;
            _workstationService = workstationService;
            _deviceService = deviceService;
            _appService = appService;
        }

        #region Server

        public string GetServerVersion()
        {
            return _appService.Version;
        }

        public async Task<int> GetDeviceTasksCount()
        {
            return await _deviceTaskService.GetCountAsync();
        }

        public async Task<List<DeviceTask>> GetDeviceTasks()
        {
            return await _deviceTaskService.Query().ToListAsync();
        }

        public async Task<List<DashboardNotify>> GetServerNotify()
        {
            var list = new List<DashboardNotify>();
            var longPendingTasksCount = await _deviceTaskService.Query().Where(d => d.CreatedAt <= DateTime.UtcNow.AddDays(-1)).CountAsync();

            if (longPendingTasksCount > 0)
            {
                list.Add(new DashboardNotify() { Message = "Long pending tasks", Count = longPendingTasksCount });
            }

            return list;
        }

        #endregion

        #region Employees

        public async Task<int> GetEmployeesCount()
        {
            return await _employeeService.GetCountAsync();
        }

        public async Task<int> GetEmployeesOpenedSessionsCount()
        {
            return await _workstationSessionService.GetOpenedSessionsCountAsync();
        }

        #endregion

        #region Devices

        public async Task<int> GetDevicesCount()
        {
            return await _deviceService.GetCountAsync();
        }

        public async Task<int> GetFreeDevicesCount()
        {
            return await _deviceService.GetFreeDevicesCount();
        }

        #endregion

        #region Workstations

        public async Task<int> GetWorkstationsCount()
        {
            return await _workstationService.GetCountAsync();
        }

        public async Task<int> GetWorkstationsOnlineCount()
        {
            return await _workstationService.GetOnlineCountAsync();
        }

        #endregion
    }
}
