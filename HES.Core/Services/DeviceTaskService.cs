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
        private readonly IDeviceAccountService _deviceAccountService;

        public DeviceTaskService(IAsyncRepository<DeviceTask> deviceTaskRepository,
                                 IDeviceAccountService deviceAccountService)
        {
            _deviceTaskRepository = deviceTaskRepository;
            _deviceAccountService = deviceAccountService;
        }

        public async Task AddTaskAsync(DeviceTask deviceTask)
        {
            await _deviceTaskRepository.AddAsync(deviceTask);
        }

        public async Task AddRangeAsync(IList<DeviceTask> deviceTasks)
        {
            await _deviceTaskRepository.AddRangeAsync(deviceTasks);
        }

        public async Task UndoLastTaskAsync(string accountId)
        {
            // Current account
            var deviceAccount = await _deviceAccountService.GetByIdAsync(accountId);

            // Undo task
            var undoTask = await _deviceTaskRepository
                .Query()
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(t => t.DeviceAccountId == accountId);

            await _deviceTaskRepository.DeleteAsync(undoTask);

            // Get last task
            var lastTask = _deviceTaskRepository
                .Query()
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefault(d => d.DeviceAccountId == deviceAccount.Id);

            if (lastTask != null)
            {
                // Update account
                switch (lastTask.Operation)
                {
                    case TaskOperation.Create:
                        deviceAccount.Status = AccountStatus.Creating;
                        deviceAccount.UpdatedAt = null;
                        break;
                    case TaskOperation.Update:
                        deviceAccount.Status = AccountStatus.Updating;
                        deviceAccount.UpdatedAt = DateTime.UtcNow;
                        break;
                    case TaskOperation.Delete:
                        deviceAccount.Status = AccountStatus.Removing;
                        deviceAccount.UpdatedAt = DateTime.UtcNow;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                deviceAccount.Status = AccountStatus.Done;
                deviceAccount.UpdatedAt = DateTime.UtcNow;
            }

            await _deviceAccountService.UpdateOnlyPropAsync(deviceAccount, new string[] { "Status", "UpdatedAt" });
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