using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class Company
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}