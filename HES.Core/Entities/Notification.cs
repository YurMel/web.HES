using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class Notification
    {
        [Key]
        public string Id { get; set; }
        public NotifyType Type { get; set; }
        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; }
        public string Url { get; set; }
    }

    public enum NotifyType
    {
        Message,
        DataProtection
    }
}