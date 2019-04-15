using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceAccountService : IDeviceAccountService
    {
        private readonly IAsyncRepository<DeviceAccount> _deviceAccountRepository;
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;

        public DeviceAccountService(IAsyncRepository<DeviceAccount> deviceAccountRepository, IAsyncRepository<DeviceTask> deviceTaskRepository)
        {
            _deviceAccountRepository = deviceAccountRepository;
            _deviceTaskRepository = deviceTaskRepository;
        }

        public async Task<IList<DeviceAccount>> GetAllAsync()
        {
            return await _deviceAccountRepository.GetAllAsync();
        }

        public async Task<IList<DeviceAccount>> GetAllWhereAsync(Expression<Func<DeviceAccount, bool>> predicate)
        {
            return await _deviceAccountRepository.GetAllWhereAsync(predicate);
        }

        public async Task<IList<DeviceAccount>> GetAllIncludeAsync(params Expression<Func<DeviceAccount, object>>[] navigationProperties)
        {
            return await _deviceAccountRepository.GetAllIncludeAsync(navigationProperties);
        }

        public async Task<DeviceAccount> GetFirstOrDefaulAsync()
        {
            return await _deviceAccountRepository.GetFirstOrDefaulAsync();
        }

        public async Task<DeviceAccount> GetFirstOrDefaulAsync(Expression<Func<DeviceAccount, bool>> match)
        {
            return await _deviceAccountRepository.GetFirstOrDefaulAsync(match);
        }

        public async Task<DeviceAccount> GetFirstOrDefaulIncludeAsync(Expression<Func<DeviceAccount, bool>> where, params Expression<Func<DeviceAccount, object>>[] navigationProperties)
        {
            return await _deviceAccountRepository.GetFirstOrDefaulIncludeAsync(where, navigationProperties);
        }

        public async Task<DeviceAccount> GetByIdAsync(dynamic id)
        {
            return await _deviceAccountRepository.GetByIdAsync(id);
        }

        public async Task<DeviceAccount> AddAsync(DeviceAccount entity)
        {
            return await _deviceAccountRepository.AddAsync(entity);
        }

        public async Task<DeviceTask> AddAsync(DeviceTask entity)
        {
            return await _deviceTaskRepository.AddAsync(entity);
        }

        public async Task<IList<DeviceAccount>> AddRangeAsync(IList<DeviceAccount> entity)
        {
            return await _deviceAccountRepository.AddRangeAsync(entity);
        }

        public async Task<IList<DeviceTask>> AddRangeAsync(IList<DeviceTask> entity)
        {
            return await _deviceTaskRepository.AddRangeAsync(entity);
        }

        public async Task UpdateAsync(DeviceAccount entity)
        {
            await _deviceAccountRepository.UpdateAsync(entity);
        }

        public async Task UpdateOnlyPropAsync(DeviceAccount entity, string[] properties)
        {
            await _deviceAccountRepository.UpdateOnlyPropAsync(entity, properties);
        }

        public async Task DeleteAsync(DeviceAccount entity)
        {
            await _deviceAccountRepository.DeleteAsync(entity);
        }

        public bool Exist(Expression<Func<DeviceAccount, bool>> predicate)
        {
            return _deviceAccountRepository.Exist(predicate);
        }
    }
}