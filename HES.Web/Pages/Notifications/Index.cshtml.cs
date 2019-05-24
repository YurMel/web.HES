using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace HES.Web.Pages.Notifications
{
    public class IndexModel : PageModel
    {
        private readonly INotificationService _notificationService;

        public IList<Notification> Notifications { get; set; }

        public IndexModel(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void OnGet()
        {
            Notifications = _notificationService.GetAllNotify();
        }
    }
}