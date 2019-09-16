using Microsoft.AspNetCore.Identity;

namespace HES.Infrastructure
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}