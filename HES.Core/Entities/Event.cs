using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace HES.Core.Entities
{
    public class Event
    {
        public string Id { get; set; }
        public byte EventId { get; set; }
        public byte StatusId { get; set; }
        public string Note { get; set; }
        public string ComputerId { get; set; }
        public string UserSession { get; set; }
        public string DeviceId { get; set; }
        public string EmployeeId { get; set; }
        public string DepartmentId { get; set; }
        public string AccountId { get; set; }
        public string AccountType { get; set; }

        [ForeignKey("ComputerId")]
        public Workstation Computer { get; set; }
        [ForeignKey("DeviceId")]
        public Device Device { get; set; }
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }
}
