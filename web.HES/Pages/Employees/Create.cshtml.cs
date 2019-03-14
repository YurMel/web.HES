using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using web.HES.Data;

namespace web.HES.Pages.Employees
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
            ViewData["CompanyId"] = new SelectList(_context.Set<Company>(), "Id", "Id");
            ViewData["DepartmentId"] = new SelectList(_context.Set<Department>(), "Id", "Id");
            ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Employee Employee { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Employee.Add(Employee);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}