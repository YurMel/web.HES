using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Positions
{
    public class IndexModel : PageModel
    {
        private readonly ISettingsService _settingsService;

        public IList<Position> Positions { get; set; }
        public bool HasForeignKey { get; set; }
        
        [BindProperty]
        public Position Position { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task OnGetAsync()
        {
            Positions = await _settingsService.PositionQuery().ToListAsync();
        }

        #region Position

        public IActionResult OnGetCreatePosition()
        {
            return Partial("_CreatePosition", this);
        }

        public async Task<IActionResult> OnPostCreatePositionAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                await _settingsService.CreatePositionAsync(Position);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditPositionAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Position = await _settingsService.PositionQuery().FirstOrDefaultAsync(m => m.Id == id);

            if (Position == null)
            {
                return NotFound();
            }

            return Partial("_EditPosition", this);
        }

        public async Task<IActionResult> OnPostEditPositionAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                await _settingsService.EditPositionAsync(Position);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeletePositionAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Position = await _settingsService.PositionQuery().FirstOrDefaultAsync(m => m.Id == id);

            if (Position == null)
            {
                return NotFound();
            }

            HasForeignKey = await _settingsService.EmployeeQuery().AnyAsync(x => x.PositionId == id);

            return Partial("_DeletePosition", this);
        }

        public async Task<IActionResult> OnPostDeletePositionAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                await _settingsService.DeletePositionAsync(id);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}