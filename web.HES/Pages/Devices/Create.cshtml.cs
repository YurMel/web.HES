using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using web.HES.Data;

namespace web.HES.Pages.Devices
{
    public class CreateModel : PageModel
    {
        private readonly web.HES.Data.ApplicationDbContext _context;

        public CreateModel(web.HES.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["RegisteredUserId"] = new SelectList(_context.Users, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Device Device { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Devices.Add(Device);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}