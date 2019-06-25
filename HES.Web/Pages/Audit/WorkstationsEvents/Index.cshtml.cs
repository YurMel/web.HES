using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using HES.Core.Entities;
using HES.Infrastructure;

namespace HES.Web.Pages.Audit.WorkstationsEvents
{
    public class IndexModel : PageModel
    {
        private readonly HES.Infrastructure.ApplicationDbContext _context;

        public IndexModel(HES.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<WorkstationEvent> WorkstationEvent { get;set; }

        public async Task OnGetAsync()
        {
            WorkstationEvent = await _context.WorkstationEvents
                .Include(w => w.Department)
                .Include(w => w.Device)
                .Include(w => w.DeviceAccount)
                .Include(w => w.Employee)
                .Include(w => w.Workstation).ToListAsync();
        }
    }
}
