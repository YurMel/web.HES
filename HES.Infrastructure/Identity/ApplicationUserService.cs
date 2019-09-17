using HES.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Infrastructure.Identity
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ApplicationUserService(ApplicationDbContext context,
                                      UserManager<ApplicationUser> userManager,
                                      IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<IList<ApplicationUser>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<ApplicationUser> GetFirstOrDefaultAsync(string id)
        {
            return await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IList<ApplicationUser>> GetOnlyAdministrators()
        {
            var administrators = new List<ApplicationUser>();

            var users = await _context.Users.ToListAsync();

            foreach (var user in users)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, ApplicationRoles.AdminRole);
                if (isAdmin)
                {
                    administrators.Add(user);
                }
            }
            return administrators;
        }

        public async Task DeleteUserAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            var applicationUser = await _context.Users.FindAsync(id);

            if (applicationUser != null)
            {
                _context.Users.Remove(applicationUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
