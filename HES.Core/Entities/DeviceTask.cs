using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class DeviceTask
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Urls { get; set; }
        public string Apps { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string OtpSecret { get; set; }
        public TaskOperation Operation { get; set; }
        public DateTime CreatedAt { get; set; }
        //public bool NameChanged { get; set; }
        //public bool UrlsChanged { get; set; }
        //public bool AppsChanged { get; set; }
        //public bool LoginChanged { get; set; }
        //public bool PasswordChanged { get; set; }
        //public bool OtpSecretChanged { get; set; }
        public string DeviceAccountId { get; set; }

        [ForeignKey("DeviceAccountId")]
        public DeviceAccount DeviceAccount { get; set; }
    }

    public enum TaskOperation
    {
        Create,
        Update,
        Delete,
        Wipe
    }
}