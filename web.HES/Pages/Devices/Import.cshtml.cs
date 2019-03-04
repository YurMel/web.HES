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
        public string UploadResult { get; set; }

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
                                    var existDevices = _context.Devices.Where(z => objects.Select(m => m.Id).Contains(z.Id)).ToList(); //get all exist devices in system
                                    if (existDevices.Count > 0)
                                    {
                                        //todo
                                        //ViewBag.ExistDevices = existDevices;
                                        UploadResult = "Devices exist";
                                    }

                                    var toImport = objects.Where(z => !existDevices.Select(m => m.Id).Contains(z.Id)).Select(d => new Device()
                                    {
                                        Id = d.Id
                                        //todo
                                    }).ToList(); //devices to import in the system
                                    if (toImport.Count > 0) //add devices if count > 0
                                    {
                                        // Save devices to DB
                                        _context.AddRange(toImport);
                                        _context.SaveChanges();

                                        UploadResult = "Devices imported";
                                        //log action
                                        //Logger.SaveActionAsync(new ApplicationLog(_userManager.GetUserId(User), action: "Import devices", issuccess: true, message: Serializer.SerializeToXML(toImport)));
                                        //ViewBag.SuccessImport = toImport;
                                    }
                                }
                                else
                                {
                                    //ViewBag.Message = Resource.no_devices_import_err;
                                    UploadResult = "No devices to import";
                                }
                            }
                            catch (Exception ex)
                            {
                                //ViewBag.Message = Resource.import_device_err1 + Environment.NewLine + ex.Message +
                                //Environment.NewLine + Resource.import_device_err2;

                                UploadResult = $"Error message {ex.Message}";
                            }
                        }
                    }
                }
                else
                {
                    //ViewBag.Message = Resource.not_correct_file_format_err;
                    UploadResult = $"Error not_correct_file_format";
                    return Page();
                }
            }
            //else
            //{
            //    //ViewBag.Message = Resource.not_specified_file_err;
            //    return Page();
            //}

            return Page();
        }
    }
}