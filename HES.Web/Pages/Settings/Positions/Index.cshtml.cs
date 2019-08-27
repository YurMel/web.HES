using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Positions
{
    public class IndexModel : PageModel
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Position> Positions { get; set; }
        public bool HasForeignKey { get; set; }

        [BindProperty]
        public Position Position { get; set; }
        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ISettingsService settingsService, ILogger<IndexModel> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Positions = await _settingsService
                .PositionQuery()
                .OrderBy(p => p.Name)
                .ToListAsync();
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
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            try
            {
                await _settingsService.CreatePositionAsync(Position);
                SuccessMessage = $"Position created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditPositionAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Position = await _settingsService.PositionQuery().FirstOrDefaultAsync(m => m.Id == id);

            if (Position == null)
            {
                _logger.LogWarning("Position == null");
                return NotFound();
            }

            return Partial("_EditPosition", this);
        }

        public async Task<IActionResult> OnPostEditPositionAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            try
            {
                await _settingsService.EditPositionAsync(Position);
                SuccessMessage = $"Position updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeletePositionAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Position = await _settingsService.PositionQuery().FirstOrDefaultAsync(m => m.Id == id);

            if (Position == null)
            {
                _logger.LogWarning("Position == null");
                return NotFound();
            }

            HasForeignKey = await _settingsService.EmployeeQuery().AnyAsync(x => x.PositionId == id);

            return Partial("_DeletePosition", this);
        }

        public async Task<IActionResult> OnPostDeletePositionAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            try
            {
                await _settingsService.DeletePositionAsync(id);
                SuccessMessage = $"Position deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<JsonResult> OnGetJsonPositionAsync()
        {
            return new JsonResult(await _settingsService.PositionQuery().OrderBy(c => c.Name).ToListAsync());
        }

        #endregion
    }
}