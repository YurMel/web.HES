using HES.Core.Entities;
using System.Linq;

namespace HES.Core.Interfaces
{
    public interface IDeviceAccountService
    {
        IQueryable<DeviceAccount> Query();
    }
}