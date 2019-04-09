using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class SharedAccount
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Urls { get; set; }

        public string Apps { get; set; }

        [Required]
        public string Login { get; set; }

        public string Password { get; set; }

        [Display(Name = "Pwd updated")]
        public DateTime? PasswordChangedAt { get; set; }

        public string OtpSecret { get; set; }

        [Display(Name = "Otp updated")]
        public DateTime? OtpSecretChangedAt { get; set; }
    }
}