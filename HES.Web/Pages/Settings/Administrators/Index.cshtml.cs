using HES.Core.Interfaces;
using HES.Infrastructure;
using HES.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Administrators
{
    public class IndexModel : PageModel
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<IndexModel> _logger;
        
        public IList<ApplicationUser> ApplicationUsers { get; set; }

        [TempData]
        public string StatusMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }
        [BindProperty]
        public ApplicationUser ApplicationUser { get; set; }
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public IndexModel(IApplicationUserService applicationUserService,
                          UserManager<ApplicationUser> userManager,
                          IEmailSender emailSender,
                          ILogger<IndexModel> logger)
        {
            _applicationUserService = applicationUserService;
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            ApplicationUsers = await _applicationUserService.GetAllAsync();
        }

        #region Invite

        public IActionResult OnGetInviteAdmin()
        {
            return Partial("_Invite", this);
        }

        public async Task<IActionResult> OnPostInviteAdminAsync()
        {
            if (ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            // Create new user
            var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
            var password = Guid.NewGuid().ToString();
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                string errors = string.Empty;
                foreach (var item in result.Errors)
                {
                    errors += $"Code: {item.Code} Description: {item.Description} {Environment.NewLine}";
                }
                _logger.LogError(errors);
                ErrorMessage = errors;
                return RedirectToPage("./Index");
            }

            await _userManager.AddToRoleAsync(user, ApplicationRoles.AdminRole);

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

            StatusMessage = "Email has been sent";
            return RedirectToPage("./Index");
        }

        #endregion

        #region Delete

        public async Task<IActionResult> OnGetDeleteAdminAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            ApplicationUser = await _applicationUserService.GetFirstOrDefaultAsync(id);

            if (ApplicationUser == null)
            {
                _logger.LogWarning("ApplicationUser == null");
                return NotFound();
            }

            return Partial("_Delete", this);
        }

        public async Task<IActionResult> OnPostDeleteAdminAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            try
            {
                await _applicationUserService.DelateAdminAsync(id);
                //StatusMessage = "Removal was successful";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}