using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class SummaryByEmployees
    {
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }

        [Display(Name = "Workstations Count")]
        public int WorkstationsCount { get; set; }

        [Display(Name = "Working Days Count")]
        public int WorkingDaysCount { get; set; }

        [Display(Name = "Total Sessions Count")]
        public int TotalSessionsCount { get; set; }

        [Display(Name = "Total Sessions Duration")]
        public TimeSpan TotalSessionsDuration { get; set; }

        [Display(Name = "AVG Session Duration")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan AvgSessionDuration { get; set; }

        [Display(Name = "AVG Session Count Per Day")]
        public decimal AvgSessionCountPerDay { get; set; }
               
        [Display(Name = "AVG Working Hours Per Day")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan AvgWorkingHoursPerDay { get; set; }
    }
}