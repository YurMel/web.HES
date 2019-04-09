using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
//using HES.Web.Data;

namespace HES.Web.Pages.Employees.test
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
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
