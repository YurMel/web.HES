using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Pages.Employees.test
{
    public class IndexModel : PageModel
    {
        private readonly web.HES.Data.ApplicationDbContext _context;

        public IndexModel(web.HES.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<DeviceAccount> DeviceAccount { get;set; }

        public async Task OnGetAsync()
        {
            DeviceAccount = await _context.DeviceAccounts
                .Include(d => d.Device)
                .Include(d => d.Employee)
                .Include(d => d.SharedAccount).ToListAsync();
        }
    }
}
