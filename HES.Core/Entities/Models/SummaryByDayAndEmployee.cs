using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class SummaryByDayAndEmployee
    {
        public DateTime Date { get; set; }
        public Employee Employee { get; set; }
        public Department Department { get; set; }

        [Display(Name = "Workstations Count")]
        public int WorkstationsCount { get; set; }

        [Display(Name = "AVG Session Duration")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan AvgSessionDuration { get; set; }

        [Display(Name = "Session Count")]
        public int SessionCount { get; set; }

        [Display(Name = "Total Session Duration")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan TotalSessionDuration { get; set; }
    }
}