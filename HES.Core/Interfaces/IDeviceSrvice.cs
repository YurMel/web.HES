using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceService
    {
        Task<IList<Device>> GetDevices();
        Task<string> ImportDevices(IList<Device> devices);
    }
}
