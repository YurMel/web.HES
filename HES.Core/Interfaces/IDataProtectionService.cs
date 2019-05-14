using HES.Core.Entities;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDataProtectionService
    {
        Task ActivateDataProtectionAsync(string password);
        Task EnableDataProtectionAsync(string password);
        Task DisableDataProtectionAsync(string password);
        Task ChangeDataProtectionPasswordAsync(string oldPassword, string newPassword);
    }
}