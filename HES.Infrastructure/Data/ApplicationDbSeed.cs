﻿using HES.Core.Entities;
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
            CreateAdministrator().Wait();
            CreateDefaultDeviceAccessProfile().Wait();
        }

        private async Task CreateAdministrator()
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

        private async Task CreateDefaultDeviceAccessProfile()
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
    }
}