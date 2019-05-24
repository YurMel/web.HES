using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace HES.Core.Services
{
    public class NotificationService : INotificationService
    {
        public static bool NotifyStatus { get; private set; }

        private readonly ILogger<NotificationService> _logger;
        private List<Notification> _notifications = new List<Notification>();

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public void AddNotify(NotifyId id, string message, string url)
        {
            _notifications.Add(new Notification() { Id = id, CreatedAt = DateTime.UtcNow, Message = message, Url = url });
            SetNotify();
        }

        public void RemoveNotify(NotifyId id)
        {
            var notification = _notifications.Find(x => x.Id == id);
            _notifications.Remove(notification);
            SetNotify();
        }

        public IList<Notification> GetAllNotify()
        {
            return _notifications;
        }

        private void SetNotify()
        {
            if (_notifications.Count > 0)
            {
                NotifyStatus = true;
            }
            else
            {
                NotifyStatus = false;
            }
        }
    }
}