using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class EmployeeWizard
    {
        public Employee Employee { get; set; }
        public WorkstationAccount WorkstationAccount { get; set; }
        [Required]
        public string DeviceId { get; set; }
        public string WorkstationId { get; set; }
        public bool ProximityUnlock { get; set; }
    }
}