using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HES.Core.Entities;
using HES.Infrastructure;

namespace HES.Web.Pages.Computers
{
    public class IndexModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public IndexModel(HES.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Computer> Computer { get;set; }

        public async Task OnGetAsync()
        {
            Computer = await _context.Computers
                .Include(c => c.Company)
                .Include(c => c.Department).ToListAsync();
        }
    }
}
