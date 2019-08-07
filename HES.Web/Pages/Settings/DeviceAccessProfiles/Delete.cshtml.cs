using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HES.Core.Entities;
using HES.Infrastructure;

namespace HES.Web.Pages.Settings.DeviceAccessProfiles
{
    public class DeleteModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public DeleteModel(HES.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DeviceAccessProfile DeviceAccessProfile { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccessProfile = await _context.DeviceAccessProfiles.FirstOrDefaultAsync(m => m.Id == id);

            if (DeviceAccessProfile == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccessProfile = await _context.DeviceAccessProfiles.FindAsync(id);

            if (DeviceAccessProfile != null)
            {
                _context.DeviceAccessProfiles.Remove(DeviceAccessProfile);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
