using HES.Core.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDeviceAccountService
    {
        IQueryable<DeviceAccount> Query();
        Task<DeviceAccount> GetByIdAsync(string accountId);
        Task UpdateOnlyPropAsync(DeviceAccount deviceAccount, string[] properties);
        Task RemoveAllAccountsAsync(string deviceId);
    }
}