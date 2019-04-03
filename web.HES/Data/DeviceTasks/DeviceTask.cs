using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web.HES.Data
{
    public class DeviceTask
    {
        [Key]
        public string Id { get; set; }
        public string Password { get; set; }
        public string OtpSecret { get; set; }
        public TaskOperation Operation { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DeviceId { get; set; }
        public string DeviceAccountId { get; set; }

        [ForeignKey("DeviceId")]
        public Device Device { get; set; }
        [ForeignKey("DeviceAccountId")]
        public DeviceAccount DeviceAccount { get; set; }
    }

    public enum TaskOperation
    {
        Create,
        Update,
        Delete
    }
}