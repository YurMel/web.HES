using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web.HES.Data;

namespace web.HES.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Device> Devices { get; set; }
        public DbSet<web.HES.Data.Employee> Employee { get; set; }
        public DbSet<web.HES.Data.Company> Company { get; set; }
        public DbSet<web.HES.Data.Position> Position { get; set; }
        public DbSet<web.HES.Data.Department> Department { get; set; }
    }
}
