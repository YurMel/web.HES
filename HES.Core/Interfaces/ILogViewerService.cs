using HES.Core.Services;
using System.Collections.Generic;

namespace HES.Core.Interfaces
{
    public interface ILogViewerService
    {
        List<string> GetLogFiles();
        List<LogModel> GetSelectedLog(string name);
        string GetFilePath(string name);
    }
}