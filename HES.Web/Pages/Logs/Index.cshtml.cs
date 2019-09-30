﻿using HES.Core.Interfaces;
using HES.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;

namespace HES.Web.Pages.Logs
{
    public class IndexModel : PageModel
    {   
        private readonly ILogViewerService _logViewerService;

        public List<LogModel> SelectedLog { get; set; } = new List<LogModel>();
        public List<string> LogFiles { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ILogViewerService logViewerService)
        {
            _logViewerService = logViewerService;   
        }

        public void OnGet()
        {
            try
            {
                LogFiles = _logViewerService.GetLogFiles();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public IActionResult OnGetShowLog(string name)
        {
            if (name == null)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                SelectedLog = _logViewerService.GetSelectedLog(name);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return Partial("_LogsTable", this);
        }

        public FileResult OnGetDownloadFile(string name)
        {
            string file_path = _logViewerService.GetFilePath(name); //"C:\\Users\\CORE\\Documents\\Visual Studio Projects\\web.hes\\HES.Web\\bin\\x64\\Debug\\netcoreapp2.2\\logs\\hes-log-own-2019-09-25.log";
            string file_type = "application/octet-stream";
            string file_name = Path.GetFileName(file_path);
            return PhysicalFile(file_path, file_type, file_name);
        }
    }    
}