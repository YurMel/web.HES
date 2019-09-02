using HES.Core.Hubs;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Workstation
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Domain { get; set; }
        [Display(Name = "Client Version")]
        public string ClientVersion { get; set; }
        public string DepartmentId { get; set; }
        public string OS { get; set; }
        public string IP { get; set; }
        [Display(Name = "Last Seen")]
        public DateTime LastSeen { get; set; }
        public bool Approved { get; set; }
        public int LockProximity { get; set; } 
        public int UnlockProximity { get; set; }
        public int LockTimeout { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [NotMapped]
        public bool IsOnline => AppHub.IsWorkstationConnectedToHost(Id);
    }
}