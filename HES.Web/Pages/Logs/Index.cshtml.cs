using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;

namespace HES.Web.Pages.Logs
{
    public class IndexModel : PageModel
    {
        //private readonly IHostingEnvironment _hostingEnvironment;
        //private readonly IFileProvider _fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
        //public IDirectoryContents DirectoryContents { get; private set; }

        public List<FileModel> OwnLogs { get; set; } = new List<FileModel>();
        public List<FileModel> AllLogs { get; set; } = new List<FileModel>();
        public List<StructureModel> CurrentLog { get; set; } = new List<StructureModel>();
        public FileModel OwnLogFile { get; set; }
        public FileModel AllLogFile { get; set; }
        public string CurrentName { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        //public IndexModel(IHostingEnvironment hostingEnvironment)
        //{
        //    _hostingEnvironment = hostingEnvironment;
        //}

        public void OnGet()
        {
            GetFiles();
        }

        public IActionResult OnPostShowOwn(FileModel OwnLogFile)
        {
            if (OwnLogFile == null)
            {
                return RedirectToPage("./Index");
            }

            CurrentLog = GetCurrentLog(OwnLogFile.Path, OwnLogFile.Separator, "own");
            CurrentName = OwnLogFile.Name;
            GetFiles();

            return Page();
        }

        public IActionResult OnPostShowAll(FileModel AllLogFile)
        {
            if (AllLogFile == null)
            {
                return RedirectToPage("./Index");
            }

            CurrentLog = GetCurrentLog(AllLogFile.Path, AllLogFile.Separator, "all");
            CurrentName = AllLogFile.Name;
            GetFiles();

            return Page();
        }

        private void GetFiles()
        {
            try
            {
                //string physicalPath = string.Empty;
                //if (_hostingEnvironment.IsDevelopment())
                //{
                //    physicalPath = Path.Combine(AppContext.BaseDirectory.Substring(AppContext.BaseDirectory.IndexOf("bin")), "logs");
                //    DirectoryContents = _fileProvider.GetDirectoryContents(physicalPath);
                //}
                //else
                //{
                //    physicalPath = "logs";
                //    DirectoryContents = _fileProvider.GetDirectoryContents(physicalPath);
                //}

                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                var directory = Path.GetDirectoryName(location);
                var folder = Path.Combine(directory, "logs");
                var info = new DirectoryInfo(folder);
                FileInfo[] fileInfo = info.GetFiles("*.log");

                foreach (var item in fileInfo)
                {
                    if (item.Name.StartsWith("hes-log-own"))
                    {
                        OwnLogs.Add(new FileModel() { Name = item.Name, Path = item.FullName });
                    }
                    else
                    {
                        //AllLogs.Add(new FileModel() { Name = item.Name, Path = item.FullName });
                    }
                }

                //foreach (var item in DirectoryContents)
                //{

                //    //var logsArray = System.IO.File.ReadAllLines(item.PhysicalPath);
                //    //var logsConcat = String.Concat(logsArray);
                //    //var logsSeparated = logsConcat.Split(separator + " ");
                //    if (item.Name.StartsWith("hes-log-own"))
                //    {
                //        OwnLogs.Add(new FileModel() { Name = item.Name, Path = item.PhysicalPath });
                //    }
                //    else
                //    {
                //        AllLogs.Add(new FileModel() { Name = item.Name, Path = item.PhysicalPath });
                //    }
                //}
            }
            catch (Exception ex)
            {
                ErrorMessage += ex.Message;
            }
        }

        private List<StructureModel> GetCurrentLog(string path, string separator, string type)
        {
            List<StructureModel> list = new List<StructureModel>();

            var logs = System.IO.File.ReadAllText(path);
            var logsSeparated = logs.Split(separator + " ");

            foreach (var item in logsSeparated)
            {
                if (item != "")
                {
                    if (type == "own")
                    {
                        list.Add(new StructureModel { Date = separator + " " + item.Split("|")[0], Level = item.Split("|")[1], Logger = item.Split("|")[2], Message = item.Split("|")[3], Method = item.Split("|")[4], Url = item.Split("|")[5] });
                    }
                    else
                    {
                        list.Add(new StructureModel { Date = separator + " " + item.Split("|")[0], Level = item.Split("|")[1], Logger = item.Split("|")[2], Message = item.Split("|")[3] });
                    }
                }
            }

            return list;
        }
    }

    public class FileModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Separator { get; set; }
    }

    public class StructureModel
    {
        public string Date { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
    }
}