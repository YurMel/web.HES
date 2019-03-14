using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using web.HES.Data;

namespace web.HES.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IList<Device> Device { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Device = await _context.Devices.Include(d => d.Employee).ToListAsync();
        }
    }
}