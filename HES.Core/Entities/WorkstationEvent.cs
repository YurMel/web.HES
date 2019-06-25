using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class WorkstationEvent
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public byte EventId { get; set; }
        public byte StatusId { get; set; }
        public string Note { get; set; }
        public string WorkstationId { get; set; }
        public string UserSession { get; set; }
        public string DeviceId { get; set; }
        public string EmployeeId { get; set; }
        public string DepartmentId { get; set; }
        public string DeviceAccountId { get; set; }
        public AccountType? AccountType { get; set; }

        [ForeignKey("ComputerId")]
        public Workstation Computer { get; set; }
        [ForeignKey("DeviceId")]
        public Device Device { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        [ForeignKey("AccountId")]
        public DeviceAccount DeviceAccount { get; set; }
    }
}