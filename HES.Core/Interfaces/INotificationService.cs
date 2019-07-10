using HES.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface INotificationService
    {
        Task<bool> GetNotifyStatusAsync();
        Task AddNotifyAsync(NotifyId id, string message, string url);
        Task RemoveNotifyAsync(NotifyId id);
        Task <IList<Notification>> GetAllNotifyAsync();
    }
}