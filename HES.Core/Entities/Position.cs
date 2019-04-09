using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class Position
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}