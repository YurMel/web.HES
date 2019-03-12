using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IList<ApplicationUser> Users { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Users = await _context.Users.ToListAsync();
        }      
    }
}