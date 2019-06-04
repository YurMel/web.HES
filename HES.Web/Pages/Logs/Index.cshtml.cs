using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;

namespace HES.Web.Pages.Logs
{
    public class IndexModel : PageModel
    {
        public List<FileModel> OwnLogs { get; set; } = new List<FileModel>();
        public List<FileModel> AllLogs { get; set; } = new List<FileModel>();
        public List<StructureModel> CurrentLog { get; set; } = new List<StructureModel>();
        public FileModel OwnLogFile { get; set; }
        public FileModel AllLogFile { get; set; }
        public string CurrentName { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

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

            CurrentLog = GetLog(OwnLogFile.Path, OwnLogFile.Separator, "own");
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

            CurrentLog = GetLog(AllLogFile.Path, AllLogFile.Separator, "all");
            CurrentName = AllLogFile.Name;
            GetFiles();

            return Page();
        }

        private void GetFiles()
        {
            try
            {
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;
                //ErrorMessage += $"location {location} {Environment.NewLine}";
                var directory = System.IO.Path.GetDirectoryName(location);
                //ErrorMessage += $"directory {directory} {Environment.NewLine}";
                var folder = Path.Combine(directory, "logs");
                //ErrorMessage += $"folder {folder} {Environment.NewLine}";
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
                        AllLogs.Add(new FileModel() { Name = item.Name, Path = item.FullName });
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage += ex.Message;
            }
        }

        private List<StructureModel> GetLog(string path, string separator, string type)
        {
            List<StructureModel> list = new List<StructureModel>();

            var logsArray = System.IO.File.ReadAllLines(path);
            var logsConcat = String.Concat(logsArray);
            var logsSeparated = logsConcat.Split(separator + " ");

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