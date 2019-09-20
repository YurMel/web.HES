using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface INotificationService
    {
        IQueryable<Notification> Query();
        Task<bool> GetNotifyStatusAsync();
        Task AddNotifyAsync(NotifyType id, string message, string url);
        Task RemoveNotifyAsync(NotifyType id);
        Task <IList<Notification>> GetAllNotifyAsync();
    }
}