using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using web.HES.Data;

namespace web.HES.Pages.Settings
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        public IList<ApplicationUser> ApplicationUser { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task OnGetAsync()
        {
            ApplicationUser = await _context.Users.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Create new user
                var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
                var password = Guid.NewGuid().ToString();
                var result = await _userManager.CreateAsync(user, password);

                // Create "invite" link
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var email = Input.Email;
                var callbackUrl = Url.Page(
                   "/Account/Invite",
                    pageHandler: null,
                    values: new { area = "Identity", code, email },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Invite to HES",
                    $"Please enter your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage();
            }
            return Page();
        }
    }
}