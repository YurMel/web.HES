using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceAccessProfilesService : IDeviceAccessProfilesService
    {
        private readonly IAsyncRepository<DeviceAccessProfile> _deviceAccessProfileRepository;

        public DeviceAccessProfilesService(IAsyncRepository<DeviceAccessProfile> deviceAccessProfileRepository)
        {
            _deviceAccessProfileRepository = deviceAccessProfileRepository;
        }

        public IQueryable<DeviceAccessProfile> DeviceAccessProfilesQuery()
        {
            return _deviceAccessProfileRepository.Query();
        }

        public async Task<DeviceAccessProfile> GetByIdAsync(dynamic id)
        {
            return await _deviceAccessProfileRepository.GetByIdAsync(id);
        }

        public Task<DeviceAccessProfile> CreateProfileAsync(DeviceAccessProfile deviceAccessProfile)
        {
            throw new NotImplementedException();
        }

        public Task EditProfileAsync(DeviceAccessProfile deviceAccessProfile)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProfileAsync(string id)
        {
            throw new NotImplementedException();
        }
    }
}
