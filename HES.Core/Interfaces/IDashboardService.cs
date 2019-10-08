using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDashboardService
    {
        string GetServerVersion();
        Task<int> GetDeviceTasksCount();
        Task<List<DeviceTask>> GetDeviceTasks();
        Task<int> GetEmployeesCount();
        Task<int> GetEmployeesOpenedSessionsCount();
        Task<int> GetDevicesCount();
        Task<int> GetFreeDevicesCount();
        Task<int> GetWorkstationsCount();
        Task<int> GetWorkstationsOnlineCount();
    }
}
