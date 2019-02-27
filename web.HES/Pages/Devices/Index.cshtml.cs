using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly web.HES.Data.ApplicationDbContext _context;

        public IndexModel(web.HES.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Device> Device { get;set; }

        public async Task OnGetAsync()
        {
            Device = await _context.Devices
                .Include(d => d.User).ToListAsync();
        }
    }
}
