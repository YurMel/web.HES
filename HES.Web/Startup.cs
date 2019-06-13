using HES.Core.Entities;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Services;
using HES.Infrastructure;
using HES.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HES.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            var server = configuration["MYSQL_SRV"];
            var port = configuration["MYSQL_PORT"];
            var uid = configuration["MYSQL_UID"];
            var pwd = configuration["MYSQL_PWD"];

            if (server != null && port != null && uid != null && pwd != null)
            {
                configuration["ConnectionStrings:DefaultConnection"] = $"server={server};port={port};database=HES;uid={uid};pwd={pwd}";
            }

            var email_host = configuration["EMAIL_HOST"];
            var email_port = configuration["EMAIL_PORT"];
            var email_ssl = configuration["EMAIL_SSL"];
            var email_user = configuration["EMAIL_USER"];
            var email_pwd = configuration["EMAIL_PWD"];

            if (email_host != null && email_port != null && email_ssl != null && email_user != null && email_pwd != null)
            {
                configuration["EmailSender:Host"] = email_host;
                configuration["EmailSender:Port"] = email_port;
                configuration["EmailSender:EnableSSL"] = email_ssl;
                configuration["EmailSender:UserName"] = email_user;
                configuration["EmailSender:Password"] = email_pwd;
            }

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Services
            services.AddScoped(typeof(IAsyncRepository<>), typeof(Repository<>));

            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<ISharedAccountService, SharedAccountService>();
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IApplicationUserService, ApplicationUserService>();
            services.AddSingleton<IRemoteTaskService, RemoteTaskService>(s =>
            {
                var scope = s.CreateScope();
                var deviceAccountRepository = scope.ServiceProvider.GetService<IAsyncRepository<DeviceAccount>>();
                var deviceTaskRepository = scope.ServiceProvider.GetService<IAsyncRepository<DeviceTask>>();
                var deviceRepository = scope.ServiceProvider.GetService<IAsyncRepository<Device>>();
                var logger = scope.ServiceProvider.GetService<ILogger<RemoteTaskService>>();
                var dataProtectionRepository = scope.ServiceProvider.GetService<IDataProtectionService>();
                var hubContext = scope.ServiceProvider.GetService<IHubContext<EmployeeDetailsHub>>();
                return new RemoteTaskService(deviceAccountRepository, deviceTaskRepository, deviceRepository, logger, dataProtectionRepository, hubContext);
            });
            services.AddSingleton<IDataProtectionService, DataProtectionService>(s =>
            {
                var scope = s.CreateScope();
                var dataProtectionRepository = scope.ServiceProvider.GetService<IAsyncRepository<AppSettings>>();
                var sharedAccountRepository = scope.ServiceProvider.GetService<IAsyncRepository<SharedAccount>>();
                var deviceTaskRepository = scope.ServiceProvider.GetService<IAsyncRepository<DeviceTask>>();
                var deviceRepository = scope.ServiceProvider.GetService<IAsyncRepository<Device>>();
                var dataProtectionProvider = scope.ServiceProvider.GetService<IDataProtectionProvider>();
                var notificationService = scope.ServiceProvider.GetService<INotificationService>();
                var logger = scope.ServiceProvider.GetService<ILogger<DataProtectionService>>();
                return new DataProtectionService(dataProtectionRepository, deviceRepository, deviceTaskRepository, sharedAccountRepository, dataProtectionProvider, notificationService, logger);
            });
            services.AddSingleton<INotificationService, NotificationService>(s =>
            {
                var scope = s.CreateScope();
                var logger = scope.ServiceProvider.GetService<ILogger<NotificationService>>();
                return new NotificationService(logger);
            });


            // Crypto
            services.AddTransient<IAesCryptography, AesCryptography>();
            // Email
            services.AddSingleton<IEmailSender, EmailSender>(i =>
                 new EmailSender(
                     Configuration["EmailSender:Host"],
                     Configuration.GetValue<int>("EmailSender:Port"),
                     Configuration.GetValue<bool>("EmailSender:EnableSSL"),
                     Configuration["EmailSender:UserName"],
                     Configuration["EmailSender:Password"]));

            // Cookie
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Dismiss strong password
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireNonAlphanumeric = false;

            });

            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Mvc
            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AddPageRoute("/Employees/Index", "");
                    options.Conventions.AuthorizeFolder("/Employees");
                    options.Conventions.AuthorizeFolder("/SharedAccounts");
                    options.Conventions.AuthorizeFolder("/Templates");
                    options.Conventions.AuthorizeFolder("/Devices");
                    options.Conventions.AuthorizeFolder("/Settings");
                    options.Conventions.AuthorizeFolder("/Logs");
                    options.Conventions.AuthorizeFolder("/Notifications");
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSignalR();
            services.AddDataProtection();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseSignalR(routes =>
            {
                routes.MapHub<DeviceHub>("/deviceHub");
                routes.MapHub<AppHub>("/appHub");
                routes.MapHub<EmployeeDetailsHub>("/employeeDetailsHub");
            });
            app.UseMvc();
            app.UseStatusCodePages("text/html", "<h1>HTTP status code {0}</h1>");

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                // Apply migration
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();
                // Db seed if first run
                var userManager = scope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
                new ApplicationDbSeed(context, userManager, roleManager).Initialize();
                // Get status of data protection
                var dataProtectionService = scope.ServiceProvider.GetService<IDataProtectionService>();
                dataProtectionService.Status();
            }
        }
    }
}