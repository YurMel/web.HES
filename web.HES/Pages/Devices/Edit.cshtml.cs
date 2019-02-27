using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Pages.Devices
{
    public class EditModel : PageModel
    {
        private readonly web.HES.Data.ApplicationDbContext _context;

        public EditModel(web.HES.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Device Device { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Device = await _context.Devices
                .Include(d => d.User).FirstOrDefaultAsync(m => m.Id == id);

            if (Device == null)
            {
                return NotFound();
            }
           ViewData["RegisteredUserId"] = new SelectList(_context.Users, "Id", "Id");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Device).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(Device.Id))
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

        private bool DeviceExists(string id)
        {
            return _context.Devices.Any(e => e.Id == id);
        }
    }
}
