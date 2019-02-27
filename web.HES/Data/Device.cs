using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace web.HES.Data
{
    public class Device
    {
        [Key]
        public string Id { get; set; }
        public string MAC { get; set; }
        public string ManufacturerUserId { get; set; }
        public string Model { get; set; }
        public string BootLoaderVersion { get; set; }
        public DateTime Manufactured { get; set; }
        public string CpuSerialNo { get; set; }
        public Byte[] DeviceKey { get; set; }
        public int? BleDeviceBatchId { get; set; }
        [MaxLength(450)]
        public string RegisteredUserId { get; set; }
        [ForeignKey("RegisteredUserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
