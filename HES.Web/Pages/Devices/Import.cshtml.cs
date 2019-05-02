using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

namespace HES.Web.Pages.Devices
{
    public class ImportModel : PageModel
    {
        private readonly IDeviceService _deviceService;

        public IList<Device> DevicesExists { get; set; }
        public IList<Device> DevicesImported { get; set; }
        public string Message { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Choose file")]
            public IFormFile FileToUpload { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public ImportModel(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var key = Input.Password;
                var file = Input.FileToUpload;
                var contentType = file.ContentType;

                if (contentType == "application/octet-stream")
                {
                    using (var fileStream = file.OpenReadStream())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await fileStream.CopyToAsync(memoryStream);

                            try
                            {
                                byte[] fileContent = memoryStream.ToArray();
                                // Device import
                                var result = await _deviceService.ImportDevices(key, fileContent);
                                // Set result
                                DevicesExists = result.devicesExists;
                                DevicesImported = result.devicesImported;
                                Message = result.message;
                            }
                            catch (Exception ex)
                            {
                                Message = $"There is a problem with device import. Exception: " +
                                          $"{Environment.NewLine} {ex.Message} " +
                                          $"Please, check if you select a correct file, enter correct encryption key and try again.";
                            }
                        }
                    }
                }
                else
                {
                    Message = "Selected file is not in correct format. Please, select .hdz file and try again.";
                }
            }
            return Page();
        }
    }
}