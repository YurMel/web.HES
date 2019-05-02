using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Infrastructure.Identity
{
    public interface IApplicationUserService
    {
        Task<IList<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser> GetFirstOrDefaultAsync(string id);
        Task DelateAdminAsync(string id);
    }
}