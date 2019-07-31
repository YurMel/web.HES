using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HES.Core.Entities.Models
{
    public class EmployeeFilter
    {
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        [Display(Name = "Position")]
        public string PositionId { get; set; }
        public DateTime? LastSynced { get; set; }
        [Display(Name = "Devices Count")]
        public int DevicesCount { get; set; }
    }
}