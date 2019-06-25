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

namespace HES.Web.Pages.Audit.WorkstationsEvents
{
    public class EditModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public EditModel(HES.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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
           ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id");
           ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id");
           ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(WorkstationEvent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkstationEventExists(WorkstationEvent.Id))
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

        private bool WorkstationEventExists(string id)
        {
            return _context.WorkstationEvents.Any(e => e.Id == id);
        }
    }
}
