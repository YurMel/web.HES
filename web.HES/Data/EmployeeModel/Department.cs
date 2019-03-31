using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace web.HES.Data
{
    public class Department
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "Company")]
        public string CompanyId { get; set; }
        [Required]
        [Display(Name = "Department")]
        public string Name { get; set; }

        [ForeignKey("CompanyId")]
        public Company Company { get; set; }
    }
}