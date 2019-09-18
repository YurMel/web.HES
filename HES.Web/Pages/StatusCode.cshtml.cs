using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;

namespace HES.Web.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class StatusCodeModel : PageModel
    {
        private readonly ILogger _logger;
        public string ErrorStatusCode { get; set; }
        public string ErrorDescription { get; set; }

        public StatusCodeModel(ILogger<StatusCodeModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string code)
        {
            ErrorStatusCode = code;
            ErrorDescription = ReasonPhrases.GetReasonPhrase(Convert.ToInt32(code));
        }
    }
}