using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HES.Core.Entities;
using HES.Infrastructure;

namespace HES.Web.Pages.Audit.WorkstationsEvents
{
    public class DetailsModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public DetailsModel(HES.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        public WorkstationEvent WorkstationEvent { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WorkstationEvent = await _context.WorkstationEvents
                .Include(w => w.Department)
                .Include(w => w.Device)
                .Include(w => w.Employee).FirstOrDefaultAsync(m => m.Id == id);

            if (WorkstationEvent == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
