using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HES.Web.Pages.Settings.IdentityProvider
{
    public class IndexModel : PageModel
    {
        private readonly ISamlIdentityProviderService _samlIdentityProviderService;
        private readonly ILogger<IndexModel> _logger;

        public SamlIdentityProvider SamlIdentityProvider { get; set; }

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
    }
}