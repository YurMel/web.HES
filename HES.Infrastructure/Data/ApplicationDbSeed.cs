using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace HES.Infrastructure
{
    public class ApplicationDbSeed
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationDbSeed(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            InitAdministrator().Wait();
        }

        private async Task InitAdministrator()
        {
            var roleResult = await _roleManager.RoleExistsAsync(ApplicationRoles.AdminRole);
            if (!roleResult)
            {
                string adminName = "admin@hideez.com";
                string adminPassword = "admin";

                // Create role
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoles.AdminRole));
                // Create user
                var user = new ApplicationUser { UserName = adminName, Email = adminName, EmailConfirmed = true };
                var createResult = await _userManager.CreateAsync(user, adminPassword);
                // Add user to role
                await _userManager.AddToRoleAsync(user, ApplicationRoles.AdminRole);
            }
        }
    }
}