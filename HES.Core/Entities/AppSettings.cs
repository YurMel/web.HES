using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class AppSettings
    {
        [Key]
        public string Id { get; set; }
        public string ProtectedValue { get; set; }
    }
}