using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using web.HES.Data;

namespace web.HES.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public PagingList<ApplicationUser> Users { get; set; }


        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }


        //public async Task OnGetAsync()
        //{
        //    Users = await _context.Users.ToListAsync();
        //}

        public async Task OnGetAsync(int page = 1)
        {
            //var qry = _context.Users.AsNoTracking().OrderBy(p => p.Email);
            //var model = await PagingList.CreateAsync(qry, 10, page);
            //Users = model;

            var query = _context.Users
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .AsQueryable();

            Users = await PagingList.CreateAsync((IOrderedQueryable<ApplicationUser>)query, 3, page);
        }
    }
}
