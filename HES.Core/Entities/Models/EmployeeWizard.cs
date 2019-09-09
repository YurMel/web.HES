using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class EmployeeWizard
    {
        public Employee Employee { get; set; }
        public WorkstationAccountModel WorkstationAccount { get; set; }
        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string WorkstationId { get; set; }
        [Required]
        public bool ProximityUnlock { get; set; }
    }
}