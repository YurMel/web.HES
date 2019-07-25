using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class SummaryByWorkstations
    {
        public string Workstation { get; set; }

        [Display(Name = "Companies Count")]
        public int CompaniesCount { get; set; }

        [Display(Name = "Departments Count")]
        public int DepartmentsCount { get; set; }

        [Display(Name = "Employees Count")]
        public int EmployeesCount { get; set; }

        [Display(Name = "Total Sessions Count")]
        public int TotalSessionsCount { get; set; }

        [Display(Name = "Total Sessions Duration")]
        public TimeSpan TotalSessionsDuration { get; set; }

        [Display(Name = "AVG Session Duration")]
        public TimeSpan AvgSessionDuration { get; set; }

        [Display(Name = "AVG Total Duartion By Employee")]
        public TimeSpan AvgTotalDuartionByEmployee { get; set; }

        [Display(Name = "AVG Total Session Count By Employee")]
        public int AvgTotalSessionCountByEmployee { get; set; }
    }
}