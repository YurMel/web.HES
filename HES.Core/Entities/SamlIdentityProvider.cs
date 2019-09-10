using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class SamlIdentityProvider
    {
        [Key]
        public string Id { get; set; }
        public bool Enabled { get; set; }
        public string Url { get; set; }

        [NotMapped]
        public static string Key => "IdentityProvider";
    }    
}