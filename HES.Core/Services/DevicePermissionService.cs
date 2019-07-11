using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DevicePermissionService : IDevicePermissionService
    {
        private readonly IAsyncRepository<DevicePermission> _devicePermissionRepository;

        public DevicePermissionService(IAsyncRepository<DevicePermission> devicePermissionRepository)
        {
            _devicePermissionRepository = devicePermissionRepository;
        }

        public IQueryable<DevicePermission> DevicePermissionQuery()
        {
            return _devicePermissionRepository.Query();
        }

        public async Task<DevicePermission> DevicePermissionGetByIdAsync(dynamic id)
        {
            return await _devicePermissionRepository.GetByIdAsync(id);
        }

        public async Task CreatePermissionAsync(DevicePermission devicePermission)
        {
            if (devicePermission == null)
            {
                throw new ArgumentNullException(nameof(devicePermission));
            }

            var prermission = await _devicePermissionRepository
                .Query()
                .Where(d => d.DeviceId == devicePermission.DeviceId)
                .Where(d => d.WorkstationId == devicePermission.WorkstationId)
                .FirstOrDefaultAsync();

            if (prermission != null)
            {
                throw new Exception("Device permission already exist.");
            }

            await _devicePermissionRepository.AddAsync(devicePermission);
        }

        public async Task DeletePermissionAsync(string devicePermissionId)
        {
            if (devicePermissionId == null)
            {
                throw new ArgumentNullException(nameof(devicePermissionId));
            }

            var devicePermission = await _devicePermissionRepository.GetByIdAsync(devicePermissionId);
            if (devicePermission == null)
            {
                throw new Exception("Device permission not found");
            }

            await _devicePermissionRepository.DeleteAsync(devicePermission);
        }       
    }
}