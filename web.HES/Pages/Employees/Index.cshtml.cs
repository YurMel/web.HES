using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Pages.Employees
{
    public class IndexModel : PageModel
    {
        private readonly web.HES.Data.ApplicationDbContext _context;

        public IndexModel(web.HES.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Employee> Employee { get;set; }

        public async Task OnGetAsync()
        {
            Employee = await _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.Devices).ToListAsync();
        }
    }
}
