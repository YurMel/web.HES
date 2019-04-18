using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;
using web.HES.Services;

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

        public async Task<IActionResult> OnPostPing(string id)
        {
            id = "D0A89E6BCD8D";

            if (AppHub.IsDeviceConnectedToHost(id))
            {
                var device = await AppHub.EstablishRemoteConnection(id, 4);

                if (device != null)
                {
                    var pingData = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04 };
                    var respData = await device.Ping(pingData);

                    Debug.Assert(pingData.SequenceEqual(respData.Result));
                }
            }

            return RedirectToPage("./Index");
        }

        #region Unpair

        //public async Task<IActionResult> OnGetDeleteTemplateAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    Template = await _context.Templates.FirstOrDefaultAsync(m => m.Id == id);

        //    if (Template == null)
        //    {
        //        return NotFound();
        //    }
        //    return Partial("_DeleteTemplate", this);

        //}

        //public async Task<IActionResult> OnPostDeleteTemplateAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    Template = await _context.Templates.FindAsync(id);

        //    if (Template != null)
        //    {
        //        _context.Templates.Remove(Template);
        //        await _context.SaveChangesAsync();
        //    }

        //    return RedirectToPage("./Index");
        //}

        #endregion
    }
}