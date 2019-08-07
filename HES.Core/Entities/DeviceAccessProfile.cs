using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class DeviceAccessProfile
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name = "Created")]
        public DateTime CreatedAt { get; set; }
        [Display(Name = "Updated")]
        public DateTime? UpdatedAt { get; set; }
        public List<Device> Devices { get; set; }
        public bool ButtonBonding { get; set; }
        public bool ButtonConnection { get; set; }
        public bool ButtonNewChannel { get; set; }
        public bool ButtonNewLink { get; set; }
        public bool PinBonding { get; set; }
        public bool PinConnection { get; set; }
        public bool PinNewChannel { get; set; }
        public bool PinNewLink { get; set; }
        public bool MasterKeyBonding { get; set; }
        public bool MasterKeyConnection { get; set; }
        public bool MasterKeyNewChannel { get; set; }
        public bool MasterKeyNewLink { get; set; }
        public int PinExpiration { get; set; }
        public int PinLength { get; set; }
        public int PinTryCount { get; set; }
        public int ButtonExpirationTimeout { get; set; }
    }
}