using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceAccountService : IDeviceAccountService
    {
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;

        public DeviceAccountService(IAsyncRepository<DeviceAccount> deviceAccountRepository)
        {
            _deviceAccountRepository = deviceAccountRepository;
        }

        public IQueryable<DeviceAccount> Query()
        {
            return _deviceAccountRepository.Query();
        }

        public async Task<DeviceAccount> GetByIdAsync(string accountId)
        {
            return await _deviceAccountRepository.GetByIdAsync(accountId);
        }

        public async Task UpdateOnlyPropAsync(DeviceAccount deviceAccount, string[] properties)
        {
            await _deviceAccountRepository.UpdateOnlyPropAsync(deviceAccount, properties);
        }

        public async Task RemoveAllAccountsAsync(string deviceId)
        {
            var allAccounts = await _deviceAccountRepository
                 .Query()
                 .Where(d => d.DeviceId == deviceId)
                 .ToListAsync();

            foreach (var account in allAccounts)
            {
                account.Deleted = true;
            }

            await _deviceAccountRepository.UpdateOnlyPropAsync(allAccounts, new string[] { "Deleted" });
        }
    }
}