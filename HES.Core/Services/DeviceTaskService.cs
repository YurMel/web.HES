using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DeviceTaskService : IDeviceTaskService
    {
        private readonly IAsyncRepository<DeviceTask> _deviceTaskRepository;

        public DeviceTaskService(IAsyncRepository<DeviceTask> deviceTaskRepository)
        {
            _deviceTaskRepository = deviceTaskRepository;
        }

        public Task AddTaskAsync(DeviceTask deviceTask)
        {
            throw new NotImplementedException();
        }

        public Task AddRangeAsync(IList<DeviceTask> deviceTasks)
        {
            throw new NotImplementedException();
        }

        public Task UndoLastTaskAsync(string accountId)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveAllTasksAsync(string deviceId)
        {
            var allTasks = await _deviceTaskRepository
                .Query()
                .Where(t => t.DeviceId == deviceId)
                .ToListAsync();

            await _deviceTaskRepository.DeleteRangeAsync(allTasks);
        }
    }
}