using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Pages.Employees.test
{
    public class EditModel : PageModel
    {
        private readonly web.HES.Data.ApplicationDbContext _context;

        public EditModel(web.HES.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
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
           ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id");
           ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
           ViewData["SharedAccountId"] = new SelectList(_context.SharedAccounts, "Id", "Id");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(DeviceAccount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceAccountExists(DeviceAccount.Id))
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

        private bool DeviceAccountExists(string id)
        {
            return _context.DeviceAccounts.Any(e => e.Id == id);
        }
    }
}
