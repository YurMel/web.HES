using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Pages.Employees.test
{
    public class DetailsModel : PageModel
    {
        private readonly web.HES.Data.ApplicationDbContext _context;

        public DetailsModel(web.HES.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public DeviceAccount DeviceAccount { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeviceAccount = await _context.DeviceAccounts
                .Include(d => d.Device)
                .Include(d => d.Employee)
                .Include(d => d.SharedAccount).FirstOrDefaultAsync(m => m.Id == id);

            if (DeviceAccount == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
