using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using web.HES.Data;

namespace web.HES.Pages.Employees.test
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
            ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            ViewData["SharedAccountId"] = new SelectList(_context.SharedAccounts, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public DeviceAccount DeviceAccount { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.DeviceAccounts.Add(DeviceAccount);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}