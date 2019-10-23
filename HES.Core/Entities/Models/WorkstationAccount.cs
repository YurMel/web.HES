﻿using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities.Models
{
    public class WorkstationAccount
    {

        [Required]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Type")]
        public WorkstationAccountType AccountType { get; set; }

        [Required]
        public string Domain { get; set; }

        [Required]
        [Display(Name = "User name")]
        public string Login { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public enum WorkstationAccountType
    {
        Local,
        Domain,
        Microsoft
    }
}