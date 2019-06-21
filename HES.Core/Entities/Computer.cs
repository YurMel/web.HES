using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class Computer
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string ClientVersion { get; set; }
        public string CompanyId { get; set; }
        public string DepartmentId { get; set; }
        public string OS { get; set; }
        public string IP { get; set; }
        public DateTime LastSeen { get; set; }
        public bool Approved { get; set; }


        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
    }
}