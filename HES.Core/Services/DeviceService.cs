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

        public async Task<Device> GetFirstOrDefaulAsync()
        {
            return await _deviceRepository.GetFirstOrDefaulAsync();
        }

        public async Task<Device> GetFirstOrDefaulAsync(Expression<Func<Device, bool>> match)
        {
            return await _deviceRepository.GetFirstOrDefaulAsync(match);
        }

        public async Task<Device> GetFirstOrDefaulIncludeAsync(Expression<Func<Device, bool>> where, params Expression<Func<Device, object>>[] navigationProperties)
        {
            return await _deviceRepository.GetFirstOrDefaulIncludeAsync(where, navigationProperties);
        }

        public async Task<Device> GetByIdAsync(dynamic id)
        {
            return await _deviceRepository.GetByIdAsync(id);
        }

        public async Task<Device> AddAsync(Device entity)
        {
            return await _deviceRepository.AddAsync(entity);
        }

        public async Task<IList<Device>> AddRangeAsync(IList<Device> entity)
        {
            return await _deviceRepository.AddRangeAsync(entity);
        }

        public async Task UpdateAsync(Device entity)
        {
            await _deviceRepository.UpdateAsync(entity);
        }

        public async Task UpdateOnlyPropAsync(Device entity, string[] properties)
        {
            await _deviceRepository.UpdateOnlyPropAsync(entity, properties);
        }

        public async Task DeleteAsync(Device entity)
        {
            await _deviceRepository.DeleteAsync(entity);
        }

        public bool Exist(Expression<Func<Device, bool>> predicate)
        {
            return _deviceRepository.Exist(predicate);
        }

        public async Task ImportDevices(IList<Device> devices)
        {
            await _deviceRepository.AddRangeAsync(devices);
        }
    }
}