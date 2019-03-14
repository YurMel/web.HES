using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web.HES.Data
{
    public class Department
    {
        [Key]
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Name { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }
}