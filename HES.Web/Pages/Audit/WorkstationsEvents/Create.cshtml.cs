using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HES.Core.Entities;
using HES.Infrastructure;

namespace HES.Web.Pages.Audit.WorkstationsEvents
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
        ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Id");
        ViewData["DeviceId"] = new SelectList(_context.Devices, "Id", "Id");
        ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public WorkstationEvent WorkstationEvent { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.WorkstationEvents.Add(WorkstationEvent);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}