using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class WorkstationBinding
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "Device")]
        public string DeviceId { get; set; }
        public string WorkstationId { get; set; }
        public bool AllowRfid { get; set; }
        public bool AllowBleTap { get; set; }
        public bool AllowProximity { get; set; }

        [ForeignKey("DeviceId")]
        public Device Device { get; set; }
        [ForeignKey("WorkstationId")]
        public Workstation Workstation { get; set; }
    }
}