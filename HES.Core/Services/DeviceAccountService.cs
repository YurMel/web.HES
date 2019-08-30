using HES.Core.Entities;
using HES.Core.Interfaces;
using System.Linq;

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
    }
}