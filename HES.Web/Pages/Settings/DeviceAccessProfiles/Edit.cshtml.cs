using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HES.Core.Entities;
using HES.Infrastructure;

namespace HES.Web.Pages.Settings.DeviceAccessProfiles
{
    public class EditModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public EditModel(HES.Infrastructure.ApplicationDbContext context)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(DeviceAccessProfile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceAccessProfileExists(DeviceAccessProfile.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool DeviceAccessProfileExists(string id)
        {
            return _context.DeviceAccessProfiles.Any(e => e.Id == id);
        }
    }
}
