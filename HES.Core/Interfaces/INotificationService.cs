using HES.Core.Entities;
using System.Collections.Generic;

namespace HES.Core.Interfaces
{
    public interface INotificationService
    {
        void AddNotify(NotifyId id, string message, string url);
        void RemoveNotify(NotifyId id);
        IList<Notification> GetAllNotify();
    }
}