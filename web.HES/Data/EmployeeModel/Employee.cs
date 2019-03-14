using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web.HES.Data
{
    public class Employee
    {
        [Key]
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PositionId { get; set; }
        public string CompanyId { get; set; }
        public string DepartmentId { get; set; }
        public DateTime LastSeen { get; set; }
        public List<Device> Devices { get; set; }

        [ForeignKey("PositionId")]
        public Position Position { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}