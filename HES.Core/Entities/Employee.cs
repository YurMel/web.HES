using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Employee
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Email { get; set; }
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Department")]
        [Required]
        public string DepartmentId { get; set; }
        [Display(Name = "Position")]
        [Required]
        public string PositionId { get; set; }
        [Display(Name = "Last Seen")]
        public DateTime? LastSeen { get; set; }
        public List<Device> Devices { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        [ForeignKey("PositionId")]
        public Position Position { get; set; }

        [NotMapped]
        [Display(Name = "Name")]
        public string FullName => $"{FirstName} {LastName}";
    }
}