using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class SummaryByEmployees
    {
        public Employee Employee { get; set; }
        public Department Department { get; set; }

        [Display(Name = "Workstations Count")]
        public int WorkstationsCount { get; set; }

        [Display(Name = "AVG Session Duration")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan AvgSessionDuration { get; set; }

        [Display(Name = "Session Count Per Day")]
        public double SessionCountPerDay { get; set; }

        [Display(Name = "Total Sessions")]
        public int TotalSessions { get; set; }

        [Display(Name = "Total Session Duration Per Day")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan TotalSessionDurationPerDay { get; set; }
    }
}