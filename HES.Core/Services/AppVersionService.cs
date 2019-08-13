using HES.Core.Interfaces;
using System.Reflection;

namespace HES.Core.Services
{
    public class AppVersionService : IAppVersionService
    {
        public string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}