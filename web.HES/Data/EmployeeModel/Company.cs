using System.ComponentModel.DataAnnotations;

namespace web.HES.Data
{
    public class Company
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
    }
}