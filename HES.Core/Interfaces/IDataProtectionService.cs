using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDataProtectionService
    {
        void CheckProtectionStatus();
        Task ActivateDataProtectionAsync(string password);
        Task EnableDataProtectionAsync(string password);
        Task DisableDataProtectionAsync(string password);
        Task ChangeDataProtectionPasswordAsync(string oldPassword, string newPassword);
        string Protect(string text);
        string Unprotect(string text);
    }
}