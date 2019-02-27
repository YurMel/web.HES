using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Pages.Devices
{
    public class DetailsModel : PageModel
    {
        private readonly web.HES.Data.ApplicationDbContext _context;

        public DetailsModel(web.HES.Data.ApplicationDbContext context)
        {
            _context = context;
        }

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
            return Page();
        }
    }
}
