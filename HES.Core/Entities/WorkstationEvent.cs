using Hideez.SDK.Communication;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class WorkstationEvent
    {
        [Key]
        public string Id { get; set; }
        public DateTime Date { get; set; }
        [Display(Name = "Event")]
        public WorkstationEventId EventId { get; set; }
        [Display(Name = "Severity")]
        public WorkstationEventSeverity SeverityId { get; set; }
        public string Note { get; set; }
        public string WorkstationId { get; set; }
        [Display(Name = "Session")]
        public string UserSession { get; set; }
        public string DeviceId { get; set; }
        public string EmployeeId { get; set; } // Only available to server
        public string DepartmentId { get; set; } // Only available to server
        public string DeviceAccountId { get; set; } // Only available to server

        [ForeignKey("WorkstationId")]
        public Workstation Workstation { get; set; }
        [ForeignKey("DeviceId")]
        public Device Device { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        [ForeignKey("DeviceAccountId")]
        [Display(Name = "Account")]
        public DeviceAccount DeviceAccount { get; set; }
    }
    
}