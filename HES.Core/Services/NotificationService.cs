using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IAsyncRepository<Notification> _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger, IAsyncRepository<Notification> notificationRepository)
        {
            _logger = logger;
            _notificationRepository = notificationRepository;
        }

        public async Task<bool> GetNotifyStatusAsync()
        {
            return await _notificationRepository.Query().AsNoTracking().AnyAsync();
        }

        public async Task AddNotifyAsync(NotifyId notifyId, string message, string url)
        {
            var allNotify = await _notificationRepository.Query().ToListAsync();

            switch (notifyId)
            {
                case NotifyId.DataProtection:
                    if (!allNotify.Where(n => n.NotifyId == NotifyId.DataProtection).Any())
                    {
                        _logger.LogWarning("Data protection requires activation");
                        await _notificationRepository.AddAsync(new Notification() { NotifyId = notifyId, CreatedAt = DateTime.UtcNow, Message = message, Url = url });
                    }
                    break;
            }
        }

        public async Task RemoveNotifyAsync(NotifyId notifyId)
        {
            var notify = await _notificationRepository.Query().Where(n => n.NotifyId == notifyId).FirstOrDefaultAsync();
            await _notificationRepository.DeleteAsync(notify);
        }

        public async Task<IList<Notification>> GetAllNotifyAsync()
        {
            return await _notificationRepository.Query().AsNoTracking().ToListAsync();
        }
    }
}