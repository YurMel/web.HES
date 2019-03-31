using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using web.HES.Data;

namespace web.HES.Pages.SharedAccounts
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IList<SharedAccount> SharedAccounts { get; set; }
        public SharedAccount SharedAccount { get; set; }

        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            SharedAccounts = await _context.SharedAccounts.ToListAsync();
        }

        #region Shared Account

        public IActionResult OnGetCreateSharedAccount()
        {
            return Partial("_CreateSharedAccount", this);
        }

        public async Task<IActionResult> OnPostCreateSharedAccountAsync(SharedAccount SharedAccount, InputModel Input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            // Set password
            SharedAccount.Password = Input.Password;
            // Set password date change
            SharedAccount.PasswordChangedAt = DateTime.Now;
            // Set otp date change
            if (!string.IsNullOrWhiteSpace(SharedAccount.OtpSecret))
            {
                SharedAccount.OtpSecretChangedAt = DateTime.Now;
            }

            _context.SharedAccounts.Add(SharedAccount);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SharedAccount = await _context.SharedAccounts.FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                return NotFound();
            }
            return Partial("_EditSharedAccount", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountAsync(string id, SharedAccount SharedAccount)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            // Set id
            SharedAccount.Id = id;
            // Set modified
            _context.Entry(SharedAccount).Property("Name").IsModified = true;
            _context.Entry(SharedAccount).Property("Urls").IsModified = true;
            _context.Entry(SharedAccount).Property("Apps").IsModified = true;
            _context.Entry(SharedAccount).Property("Login").IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SharedAccountExists(SharedAccount.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountPwdAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SharedAccount = await _context.SharedAccounts.FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                return NotFound();
            }
            return Partial("_EditSharedAccountPwd", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountPwdAsync(string id, SharedAccount SharedAccount, InputModel Input)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            // Set id
            SharedAccount.Id = id;
            // Set password
            SharedAccount.Password = Input.Password;
            // Set password date change
            SharedAccount.PasswordChangedAt = DateTime.Now;
            // Set modified
            _context.Entry(SharedAccount).Property("Password").IsModified = true;
            _context.Entry(SharedAccount).Property("PasswordChangedAt").IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SharedAccountExists(SharedAccount.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditSharedAccountOtpAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SharedAccount = await _context.SharedAccounts.FirstOrDefaultAsync(m => m.Id == id);

            if (SharedAccount == null)
            {
                return NotFound();
            }

            return Partial("_EditSharedAccountOtp", this);
        }

        public async Task<IActionResult> OnPostEditSharedAccountOtpAsync(string id, SharedAccount SharedAccount)
        {
            // Set id
            SharedAccount.Id = id;
            // Set otp date change
            if (!string.IsNullOrWhiteSpace(SharedAccount.OtpSecret))
            {
                SharedAccount.OtpSecretChangedAt = DateTime.Now;
            }
            else
            {
                SharedAccount.OtpSecretChangedAt = null;
            }
            // Set modified
            _context.Entry(SharedAccount).Property("OtpSecret").IsModified = true;
            _context.Entry(SharedAccount).Property("OtpSecretChangedAt").IsModified = true;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SharedAccountExists(SharedAccount.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool SharedAccountExists(string id)
        {
            return _context.SharedAccounts.Any(e => e.Id == id);
        }

        public async Task<IActionResult> OnGetDeleteSharedAccountAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SharedAccount = await _context.SharedAccounts.FirstOrDefaultAsync(m => m.Id == id);

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

            SharedAccount = await _context.SharedAccounts.FindAsync(id);

            if (SharedAccount != null)
            {
                _context.SharedAccounts.Remove(SharedAccount);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}