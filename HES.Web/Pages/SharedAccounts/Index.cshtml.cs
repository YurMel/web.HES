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

namespace HES.Web.Pages.SharedAccounts
{
    public class IndexModel : PageModel
    {
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IRemoteTaskService _remoteTaskService;
        private readonly IRemoteWorkstationConnectionsService _remoteWorkstationConnectionsService;
        private readonly ILogger<IndexModel> _logger;

        public IList<SharedAccount> SharedAccounts { get; set; }
        public SharedAccount SharedAccount { get; set; }
        public InputModel Input { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ISharedAccountService sharedAccountService,
                          IRemoteTaskService remoteTaskService,
                          IRemoteWorkstationConnectionsService remoteWorkstationConnectionsService,
                          ILogger<IndexModel> logger)
        {
            _sharedAccountService = sharedAccountService;
            _remoteTaskService = remoteTaskService;
            _remoteWorkstationConnectionsService = remoteWorkstationConnectionsService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            SharedAccounts = await _sharedAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .ToListAsync();
        }

        #region Shared Account

        public IActionResult OnGetCreateSharedAccount()
        {
            return Partial("_CreateSharedAccount", this);
        }

        public async Task<IActionResult> OnPostCreateSharedAccountAsync(SharedAccount sharedAccount, InputModel input)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            if (!Core.Utilities.Hepler.VerifyOtpSecret(input.OtpSecret))
            {
                _logger.LogWarning("OTP secret is not valid");
                ErrorMessage = "OTP secret is not valid.";
                return RedirectToPage("./Index");
            }

            try
            {
                await _sharedAccountService.CreateSharedAccountAsync(sharedAccount, input);
                SuccessMessage = $"Shared account created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            SharedAccount = await _sharedAccountService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                _logger.LogWarning("SharedAccount == null");
                return NotFound();
            }

            return Partial("_EditSharedAccount", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountAsync(SharedAccount sharedAccount)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            try
            {
                var devices = await _sharedAccountService.EditSharedAccountAsync(sharedAccount);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                SuccessMessage = $"Shared account updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountPwdAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            SharedAccount = await _sharedAccountService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                _logger.LogWarning("SharedAccount == null");
                return NotFound();
            }
            return Partial("_EditSharedAccountPwd", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountPwdAsync(SharedAccount sharedAccount, InputModel input)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model is not valid");
                return RedirectToPage("./Index");
            }

            try
            {
                var devices = await _sharedAccountService.EditSharedAccountPwdAsync(sharedAccount, input);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                SuccessMessage = $"Shared account updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountOtpAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            SharedAccount = await _sharedAccountService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                _logger.LogWarning("SharedAccount == null");
                return NotFound();
            }

            return Partial("_EditSharedAccountOtp", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountOtpAsync(SharedAccount sharedAccount, InputModel input)
        {
            if (!Core.Utilities.Hepler.VerifyOtpSecret(input.OtpSecret))
            {
                _logger.LogWarning("OTP secret is not valid");
                ErrorMessage = "OTP secret is not valid.";
                return RedirectToPage("./Index");
            }

            try
            {
                var devices = await _sharedAccountService.EditSharedAccountOtpAsync(sharedAccount, input);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                SuccessMessage = $"Shared account updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteSharedAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            SharedAccount = await _sharedAccountService
                .Query()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                _logger.LogWarning("SharedAccount == null");
                return NotFound();
            }
            return Partial("_DeleteSharedAccount", this);
        }

        public async Task<IActionResult> OnPostDeleteSharedAccountAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            try
            {
                var devices = await _sharedAccountService.DeleteSharedAccountAsync(id);
                _remoteWorkstationConnectionsService.StartUpdateRemoteDevice(devices);
                SuccessMessage = $"Shared account deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}