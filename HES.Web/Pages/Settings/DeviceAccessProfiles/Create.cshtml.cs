using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HES.Core.Entities;
using HES.Infrastructure;

namespace HES.Web.Pages.Settings.DeviceAccessProfiles
{
    public class CreateModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public CreateModel(HES.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public DeviceAccessProfile DeviceAccessProfile { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.DeviceAccessProfiles.Add(DeviceAccessProfile);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}