using HES.Core.Services;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDataProtectionService
    {
        void Status();
        ProtectionStatus GetStatus();
        bool CanUse();
        Task ActivateDataProtectionAsync(string password, string user);
        Task EnableDataProtectionAsync(string password, string user);
        Task DisableDataProtectionAsync(string password, string user);
        Task ChangeDataProtectionPasswordAsync(string oldPassword, string newPassword, string user);
        string Protect(string text);
        string Unprotect(string text);
        void Validate();
    }
}