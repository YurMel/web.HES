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

namespace HES.Web.Pages.Computers
{
    public class EditModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public EditModel(HES.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Computer Computer { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Computer = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.Department).FirstOrDefaultAsync(m => m.Id == id);

            if (Computer == null)
            {
                return NotFound();
            }
           ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Id");
           ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Computer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComputerExists(Computer.Id))
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

        private bool ComputerExists(string id)
        {
            return _context.Computers.Any(e => e.Id == id);
        }
    }
}
