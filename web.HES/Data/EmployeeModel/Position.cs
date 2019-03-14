using System.ComponentModel.DataAnnotations;

namespace web.HES.Data
{
    public class Position
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}