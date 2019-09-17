using HES.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(SignInManager<ApplicationUser> signInManager,
                                  UserManager<ApplicationUser> userManager,
                                  ILogger<IdentityController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> AuthN(string email, string password, string otp)
        {
            if (email == null && password == null)
            {
                return BadRequest(new { error = "CredentialsNullException" });
            }

            // Find user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new { error = "UserNotFoundException" });
            }

            // Two factor requires
            if (user.TwoFactorEnabled && otp == null)
            {
                return Ok(new { message = "UserRequiresTwoFactor" });
            }

            // Sing In
            var result = await _signInManager.PasswordSignInAsync(email, password, false, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation($"User {email} succeeded auth via API");

                var response = new User()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber
                };

                return Ok(response);
            }
            if (result.RequiresTwoFactor)
            {
                var authenticatorCode = otp.Replace(" ", string.Empty).Replace("-", string.Empty);

                var twoFactorResult = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, false, false);

                if (twoFactorResult.Succeeded)
                {
                    _logger.LogInformation($"User {email} succeeded auth via API with 2FA");

                    var response = new User()
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber
                    };

                    return Ok(response);
                }
                else if (twoFactorResult.IsLockedOut)
                {
                    _logger.LogWarning($"User {user.Email} account locked out.");
                    return BadRequest(new { error = "UserIsLockedoutException" });
                }
                else
                {
                    _logger.LogWarning($"Invalid authenticator code entered for user with ID {user.Id}, email {user.Email}.");
                    return Unauthorized(new { error = "InvalidAuthenticatorCodeException" });
                }

            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning($"User {user.Email} account locked out.");
                return BadRequest(new { error = "UserIsLockedoutException" });
            }
            else
            {
                _logger.LogError($"User {user.Email} unauthorized.");
                return Unauthorized();
            }
        }
    }

    class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}