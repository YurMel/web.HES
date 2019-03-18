using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using web.HES.Data;
using web.HES.Helpers.Interfaces;

namespace web.HES.Pages.Devices
{
    public class ImportModel : PageModel
    {
        private readonly IAesCryptography _aes;
        private readonly ApplicationDbContext _context;
        public IList<Device> DevicesExist { get; set; }
        public IList<Device> DevicesImported { get; set; }
        public string Message { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public IFormFile FileToUpload { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public class MyHideezDevice
        {
            public string Id { get; set; }
            public string MAC { get; set; }
            public string ManufacturerUserId { get; set; }
            public string Model { get; set; }
            public string BootLoaderVersion { get; set; }
            public DateTime Manufactured { get; set; }
            public string CpuSerialNo { get; set; }
            public Byte[] DeviceKey { get; set; }
            public int? BleDeviceBatchId { get; set; }
            public string RegisteredUserId { get; set; }
            public virtual ApplicationUser User { get; set; }
        }

        public ImportModel(ApplicationDbContext context, IAesCryptography aes)
        {
            _context = context;
            _aes = aes;
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
                                var objects = _aes.DecryptObject<List<MyHideezDevice>>(fileContent, Encoding.Unicode.GetBytes(key));
                                if (objects.Count > 0)
                                {
                                    // Get all exist devices in system
                                    var isExist = _context.Devices.Where(z => objects.Select(m => m.Id).Contains(z.Id)).ToList();
                                    if (isExist.Count > 0)
                                    {
                                        DevicesExist = isExist;
                                    }

                                    // Devices to import in the system
                                    var toImport = objects.Where(z => !isExist.Select(m => m.Id).Contains(z.Id)).Select(d => new Device()
                                    {
                                        Id = d.Id,
                                        MAC = d.MAC,
                                        Model = d.Model,
                                        ImportedAt = DateTime.Now,
                                        DeviceKey = d.DeviceKey,
                                        RFID = null,
                                        EmployeeId = null,
                                        Battery = 0,
                                        Firmware = null,
                                        LastSynced = DateTime.Now
                                    }).ToList();

                                    // Add devices if count > 0
                                    if (toImport.Count > 0)
                                    {
                                        // Save devices to DB
                                        _context.AddRange(toImport);
                                        _context.SaveChanges();

                                        DevicesImported = toImport;
                                        //Logger.SaveActionAsync(new ApplicationLog(_userManager.GetUserId(User), action: "Import devices", issuccess: true, message: Serializer.SerializeToXML(toImport)));
                                    }
                                }
                                else
                                {
                                    Message = "File is recognized, but it is no devices to import. Check file structure and try again.";
                                }
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