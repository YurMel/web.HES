using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class SummaryByDepartments
    {
        public Department Department { get; set; }

        [Display(Name = "Employees Count")]
        public int EmployeesCount { get; set; }

        [Display(Name = "Workstations Count")]
        public int WorkstationsCount { get; set; }

        [Display(Name = "AVG Session Duration")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan AvgSessionDuration { get; set; }

        [Display(Name = "Session Count Per Day")]
        public int SessionCountPerDay { get; set; }

        [Display(Name = "Total Sessions")]
        public int TotalSessions { get; set; }

        [Display(Name = "Total Session Duration Per Day")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan TotalSessionDurationPerDay { get; set; }
    }
}