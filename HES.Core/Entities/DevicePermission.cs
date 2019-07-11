using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class DevicePermission
    {
        [Key]
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string WorkstationId { get; set; }
        public bool AllowRfid { get; set; }
        public bool AllowBleTap { get; set; }
        public bool AllowProximity { get; set; }
    }
}