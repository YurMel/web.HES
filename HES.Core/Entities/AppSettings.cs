using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class AppSettings
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}