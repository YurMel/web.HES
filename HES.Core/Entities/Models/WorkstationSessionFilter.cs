using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class WorkstationSessionFilter
    {
        [Display(Name = "Start Time")]
        public DateTime? StartTime { get; set; }
        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }
        public TimeSpan? Duration { get; set; }
        [Display(Name = "Unlocked by")]
        public int? UnlockId { get; set; }
        [Display(Name = "Workstation")]
        public string WorkstationId { get; set; }
        [Display(Name = "Session")]
        public string UserSession { get; set; }
        [Display(Name = "Device")]
        public string DeviceId { get; set; }
        [Display(Name = "Employee")]
        public string EmployeeId { get; set; }
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Display(Name = "Department")]
        public string DepartmentId { get; set; }
        [Display(Name = "Account")]
        public string DeviceAccountId { get; set; }
        [Display(Name = "Account Type")]
        public int? DeviceAccountTypeId { get; set; }
    }
}