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
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IAsyncRepository<Notification> notificationRepository,
                                   IApplicationUserService applicationUserService,
                                   ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _applicationUserService = applicationUserService;
            _logger = logger;
        }

        public IQueryable<Notification> Query()
        {
            return _notificationRepository.Query();
        }

        public async Task<bool> GetNotifyStatusAsync()
        {
            return await _notificationRepository.Query().AsNoTracking().AnyAsync();
        }

        public async Task<IList<Notification>> GetAllNotifyAsync()
        {
            return await _notificationRepository.Query().AsNoTracking().ToListAsync();
        }

        public async Task AddNotifyAsync(NotifyType type, string message, string url)
        {
            switch (type)
            {
                case NotifyType.Message:
                    await _notificationRepository.AddAsync(new Notification() { Type = type, CreatedAt = DateTime.UtcNow, Message = message, Url = url });
                    break;
                case NotifyType.DataProtection:
                    var dp = await _notificationRepository.Query().FirstOrDefaultAsync(d => d.Type == NotifyType.DataProtection);
                    if (dp == null)
                    {
                        await _notificationRepository.AddAsync(new Notification() { Type = type, CreatedAt = DateTime.UtcNow, Message = message, Url = url });
                        await _applicationUserService.SendEmailDataProtectionNotify();
                    }
                    break;
            }
        }

        public async Task RemoveNotifyAsync(NotifyType type)
        {
            var notify = await _notificationRepository.Query().Where(n => n.Type == type).FirstOrDefaultAsync();
            await _notificationRepository.DeleteAsync(notify);
        }
    }
}