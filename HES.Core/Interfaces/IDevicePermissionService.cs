using HES.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDevicePermissionService
    {
        IQueryable<DevicePermission> DevicePermissionQuery();
        Task<DevicePermission> DevicePermissionGetByIdAsync(dynamic id);
        Task CreatePermissionAsync(DevicePermission devicePermission);
        Task DeletePermissionAsync(string devicePermissionId);
    }
}