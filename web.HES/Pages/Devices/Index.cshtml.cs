using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;
using web.HES.Helpers;

namespace web.HES.Pages.Devices
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public PaginatedList<Device> Device { get; set; }
        public string MacSort { get; set; }
        public string ManufacturerSort { get; set; }
        public string DateSort { get; set; }
        public string CurrentFilter { get; set; }
        public string CurrentSort { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        //public IList<Device> Device { get;set; }

        //public async Task OnGetAsync()
        //{
        //    Device = await _context.Devices
        //        .Include(d => d.User).ToListAsync();
        //}

        public async Task OnGetAsync(string sortOrder, string currentFilter, string searchString, int? pageIndex)
        {
            CurrentSort = sortOrder;
            MacSort = String.IsNullOrEmpty(sortOrder) ? "mac_desc" : "";
            ManufacturerSort = sortOrder == "Manufacturer" ? "manufacturer_desc" : "Manufacturer";
            DateSort = sortOrder == "Date" ? "date_desc" : "Date";
            if (searchString != null)
            {
                pageIndex = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            CurrentFilter = searchString;

            IQueryable<Device> deviceIQ = from s in _context.Devices.Include(d => d.User) select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                deviceIQ = deviceIQ.Where(s => s.MAC.Contains(searchString) || s.ManufacturerUserId.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "mac_desc":
                    deviceIQ = deviceIQ.OrderByDescending(s => s.MAC);
                    break;
                case "Manufacturer":
                    deviceIQ = deviceIQ.OrderBy(s => s.ManufacturerUserId);
                    break;
                case "manufacturer_desc":
                    deviceIQ = deviceIQ.OrderByDescending(s => s.ManufacturerUserId);
                    break;
                case "Date":
                    deviceIQ = deviceIQ.OrderBy(s => s.BootLoaderVersion);
                    break;
                case "date_desc":
                    deviceIQ = deviceIQ.OrderByDescending(s => s.BootLoaderVersion);
                    break;
                default:
                    deviceIQ = deviceIQ.OrderBy(s => s.MAC);
                    break;
            }

            int pageSize = 10;
            Device = await PaginatedList<Device>.CreateAsync(deviceIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
        }
    }
}
