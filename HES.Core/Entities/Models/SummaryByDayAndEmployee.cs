using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class SummaryByDayAndEmployee
    {
        public DateTime Date { get; set; }
        public string Employee { get; set; }
        public string Company { get; set; }
        public string Department { get; set; }
        [Display(Name = "Workstations Count")]
        public int WorkstationsCount { get; set; }
        [Display(Name = "AVG Session Duration")]
        public TimeSpan AvgSessionDuration { get; set; }
        [Display(Name = "Session Count")]
        public int SessionCount { get; set; }
        [Display(Name = "Total Session Duration")]
        public TimeSpan TotalSessionDuration { get; set; }
    }
}