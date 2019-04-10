using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IAsyncRepository<Device> _deviceRepository;

        public DeviceService(IAsyncRepository<Device> deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<IList<Device>> GetAllAsync()
        {
            return await _deviceRepository.GetAllAsync();
        }

        public async Task<IList<Device>> GetAllWhereAsync(Expression<Func<Device, bool>> predicate)
        {
            return await _deviceRepository.GetAllWhereAsync(predicate);
        }

        public async Task<IList<Device>> GetAllIncludeAsync(params Expression<Func<Device, object>>[] navigationProperties)
        {
            return await _deviceRepository.GetAllIncludeAsync(navigationProperties);
        }

        public async Task ImportDevices(IList<Device> devices)
        {
            await _deviceRepository.AddRangeAsync(devices);
        }
    }
}