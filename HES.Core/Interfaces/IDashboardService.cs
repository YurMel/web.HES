using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDashboardService
    {
        Task<int> GetDeviceTasksCount();
        Task<List<DeviceTask>> GetDeviceTasks();
    }
}
