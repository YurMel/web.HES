using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
//using HES.Web.Data;

namespace HES.Web.Pages.Employees.test
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
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
