using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
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

        public async Task<IList<Device>> GetDevices()
        {
            return await _deviceRepository.GetAllIncludeAsync(d => d.Employee);
        }

        public Task<string> ImportDevices(IList<Device> devices)
        {
            throw new NotImplementedException();
        }
    }
}
