using Microsoft.AspNetCore.Identity;

namespace web.HES.Data
{
    public class ApplicationUser : IdentityUser
    {
        public int TestField { get; set; }
    }
}