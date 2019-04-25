using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Device
    {
        [Display(Name = "ID")]
        [Key]
        public string Id { get; set; }
        public string MAC { get; set; }
        public string Model { get; set; }
        public string RFID { get; set; }
        public int Battery { get; set; }
        public string Firmware { get; set; }
        public DateTime? LastSynced { get; set; }
        public string EmployeeId { get; set; }
        public string PrimaryAccountId { get; set; }
        public DateTime ImportedAt { get; set; }
        public byte[] DeviceKey { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
    }
}