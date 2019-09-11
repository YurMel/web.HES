using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.IdentityProvider
{
    public class IndexModel : PageModel
    {
        private readonly ISamlIdentityProviderService _samlIdentityProviderService;
        private readonly ILogger<IndexModel> _logger;

        public SamlIdentityProvider SamlIdentityProvider { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ISamlIdentityProviderService samlIdentityProviderService, ILogger<IndexModel> logger)
        {
            _samlIdentityProviderService = samlIdentityProviderService;
            _logger = logger;
        }

        public async Task OnGet()
        {
            SamlIdentityProvider = await _samlIdentityProviderService
                .Query()
                .FirstOrDefaultAsync();
        }

        public async Task<IActionResult> OnPostEditSamlIdentityProviderAsync(SamlIdentityProvider samlIdentityProvider)
        {
            try
            {
                await _samlIdentityProviderService.EditSamlIdentityProviderAsync(samlIdentityProvider);
                SuccessMessage = $"SAML IdP settings updated.";
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }
            return RedirectToPage("./Index");
        }
    }
}