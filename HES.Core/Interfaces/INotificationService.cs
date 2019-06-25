using HES.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface INotificationService
    {
        Task<bool> GetNotifyStatus();
        Task AddNotify(NotifyId id, string message, string url);
        Task RemoveNotify(NotifyId id);
        Task <IList<Notification>> GetAllNotify();
    }
}