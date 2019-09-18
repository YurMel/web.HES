using HES.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace HES.Infrastructure
{
    public class ApplicationDbSeed
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationDbSeed(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            InitRoleAndUser().Wait();
            InitDefaultDeviceAccessProfile().Wait();
            InitIdentityProviderSettings().Wait();
        }

        private async Task InitRoleAndUser()
        {
            var roleResult = await _roleManager.RoleExistsAsync(ApplicationRoles.AdminRole);
            if (!roleResult)
            {
                string adminName = "admin@hideez.com";
                string adminPassword = "admin";

                // Create role
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoles.AdminRole));
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoles.UserRole));
                // Create user
                var user = new ApplicationUser { UserName = adminName, Email = adminName, EmailConfirmed = true };
                var createResult = await _userManager.CreateAsync(user, adminPassword);
                // Add user to role
                await _userManager.AddToRoleAsync(user, ApplicationRoles.AdminRole);
            }
        }

        private async Task InitDefaultDeviceAccessProfile()
        {
            var profile = await _context.DeviceAccessProfiles.FindAsync("default");

            if (profile == null)
            {
                await _context.DeviceAccessProfiles.AddAsync(new DeviceAccessProfile
                {
                    Id = "default",
                    Name = "Default",
                    CreatedAt = DateTime.UtcNow,
                    ButtonBonding = true,
                    ButtonConnection = false,
                    ButtonNewChannel = false,
                    ButtonNewLink = false,
                    PinBonding = false,
                    PinConnection = true,
                    PinNewChannel = true,
                    PinNewLink = true,
                    MasterKeyBonding = true,
                    MasterKeyConnection = false,
                    MasterKeyNewChannel = false,
                    MasterKeyNewLink = false,
                    PinExpiration = 86400,
                    PinLength = 4,
                    PinTryCount = 10,
                    MasterKeyExpiration = 86400,
                    ButtonExpiration = 15
                });

                await _context.SaveChangesAsync();
            }
        }

        private async Task InitIdentityProviderSettings()
        {
            var samlIdPEnabled = await _context.SamlIdentityProvider.FindAsync(SamlIdentityProvider.Key);
            if (samlIdPEnabled == null)
            {
                await _context.SamlIdentityProvider.AddAsync(new SamlIdentityProvider
                {
                    Id = SamlIdentityProvider.Key,
                    Enabled = false,
                    Url = "url"
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}