using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class Notification
    {
        [Key]
        public NotifyId Id { get; set; }
        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; }
        [Display(Name = "Action")]
        public string Url { get; set; }
    }

    public enum NotifyId
    {
        data_protection
    }
}