using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class Template
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Urls { get; set; }
        public string Apps { get; set; }
    }
}