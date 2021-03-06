﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DeviceAccessProfiles
{
    public class IndexModel : PageModel
    {
        private readonly IDeviceAccessProfilesService _deviceAccessProfilesService;
        private readonly ILogger<IndexModel> _logger;

        public IList<DeviceAccessProfile> DeviceAccessProfiles { get; set; }
        public DeviceAccessProfile DeviceAccessProfile { get; set; }
        public bool ProfileHasForeignKey { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IDeviceAccessProfilesService deviceAccessProfilesService, ILogger<IndexModel> logger)
        {
            _deviceAccessProfilesService = deviceAccessProfilesService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            DeviceAccessProfiles = await _deviceAccessProfilesService
                .DeviceAccessProfilesQuery()
                .Include(d => d.Devices)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetDetailsProfileAsync(string id)
        {
            DeviceAccessProfile = await _deviceAccessProfilesService.GetByIdAsync(id);
            return Partial("_DetailsProfile", this);
        }

        public IActionResult OnGetCreateProfile()
        {
            return Partial("_CreateProfile", this);
        }

        public async Task<IActionResult> OnPostCreateProfileAsync(DeviceAccessProfile DeviceAccessProfile)
        {
            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning("DeviceAccessProfile == null");
                return RedirectToPage("./Index");
            }

            try
            {
                await _deviceAccessProfilesService.CreateProfileAsync(DeviceAccessProfile);
                SuccessMessage = $"Device profile created.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditProfileAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccessProfile = await _deviceAccessProfilesService
                .DeviceAccessProfilesQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning("DeviceAccessProfile == null");
                return NotFound();
            }

            return Partial("_EditProfile", this);
        }

        public async Task<IActionResult> OnPostEditProfileAsync(DeviceAccessProfile DeviceAccessProfile)
        {
            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning("DeviceAccessProfile == null");
                return RedirectToPage("./Index");
            }

            try
            {
                await _deviceAccessProfilesService.EditProfileAsync(DeviceAccessProfile);
                SuccessMessage = $"Device access profile updated.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteProfileAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            DeviceAccessProfile = await _deviceAccessProfilesService
                .DeviceAccessProfilesQuery()
                .Include(d => d.Devices)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (DeviceAccessProfile == null)
            {
                _logger.LogWarning("DeviceAccessProfile == null");
                return NotFound();
            }

            ProfileHasForeignKey = DeviceAccessProfile.Devices.Count == 0 ? false : true;

            return Partial("_DeleteProfile", this);
        }

        public async Task<IActionResult> OnPostDeleteProfileAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            try
            {
                await _deviceAccessProfilesService.DeleteProfileAsync(id);
                SuccessMessage = $"Device access profile deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }
    }
}
