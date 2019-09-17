using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Infrastructure.Identity
{
    public interface IApplicationUserService
    {
        Task<IList<ApplicationUser>> GetAllAsync();
        Task<ApplicationUser> GetFirstOrDefaultAsync(string id);
        Task<IList<ApplicationUser>> GetOnlyAdministrators();
        Task DeleteUserAsync(string id);
    }
}