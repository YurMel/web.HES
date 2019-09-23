using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IWorkstationService _workstationService;
        private readonly IDeviceService _deviceService;

        public DashboardService(IDeviceTaskService deviceTaskService,
                                IWorkstationService workstationService,
                                IDeviceService deviceService)
        {
            _deviceTaskService = deviceTaskService;
            _workstationService = workstationService;
            _deviceService = deviceService;
        }


        public async Task<int> GetDeviceTasksCount()
        {
            var tasks = await _deviceTaskService.Query().ToListAsync();
            return tasks.Count;
        }

        public async Task<List<DeviceTask>> GetDeviceTasks()
        {
            return await _deviceTaskService.Query().ToListAsync();
        }
    }
}
