using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmartBreadcrumbs.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    [Breadcrumb("SharedAccounts")]
    public class IndexModel : PageModel
    {
        private readonly ISharedAccountService _sharedAccountService;
        public IList<SharedAccount> SharedAccounts { get; set; }
        public SharedAccount SharedAccount { get; set; }
        public InputModel Input { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ISharedAccountService sharedAccountService)
        {
            _sharedAccountService = sharedAccountService;
        }

        public async Task OnGetAsync()
        {
            SharedAccounts = await _sharedAccountService.GetAllWhereAsync(d => d.Deleted == false);
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
                return RedirectToPage("./Index");
            }

            try
            {
                await _sharedAccountService.CreateSharedAccountAsync(sharedAccount, input);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SharedAccount = await _sharedAccountService.GetFirstOrDefaulAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                return NotFound();
            }

            return Partial("_EditSharedAccount", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountAsync(SharedAccount sharedAccount)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                await _sharedAccountService.EditSharedAccountAsync(sharedAccount);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountPwdAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SharedAccount = await _sharedAccountService.GetFirstOrDefaulAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                return NotFound();
            }
            return Partial("_EditSharedAccountPwd", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountPwdAsync(SharedAccount sharedAccount, InputModel input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                await _sharedAccountService.EditSharedAccountPwdAsync(sharedAccount, input);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountOtpAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SharedAccount = await _sharedAccountService.GetFirstOrDefaulAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                return NotFound();
            }

            return Partial("_EditSharedAccountOtp", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountOtpAsync(SharedAccount sharedAccount)
        {
            try
            {
                await _sharedAccountService.EditSharedAccountOtpAsync(sharedAccount);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }
               
        public async Task<IActionResult> OnGetDeleteSharedAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SharedAccount = await _sharedAccountService.GetFirstOrDefaulAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                return NotFound();
            }
            return Partial("_DeleteSharedAccount", this);
        }

        public async Task<IActionResult> OnPostDeleteSharedAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                await _sharedAccountService.DeleteSharedAccountAsync(id);
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