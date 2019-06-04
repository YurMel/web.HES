using HES.Core.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Infrastructure
{
    public class ApplicationDbSeed
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationDbSeed(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
            InitAdministrator().Wait();
        }

        private async Task InitAdministrator()
        {
            var roleResult = await _roleManager.RoleExistsAsync(ApplicationRoles.AdminRole);
            if (!roleResult)
            {
                string adminName = "admin@hideez.com";
                string adminPassword = "admin";

                // Create role
                await _roleManager.CreateAsync(new IdentityRole(ApplicationRoles.AdminRole));
                // Create user
                var user = new ApplicationUser { UserName = adminName, Email = adminName, EmailConfirmed = true };
                var createResult = await _userManager.CreateAsync(user, adminPassword);
                // Add user to role
                await _userManager.AddToRoleAsync(user, ApplicationRoles.AdminRole);
                // Create data
                await InitData();
            }
        }

        private async Task InitData()
        {
            // Add devices
            var devices = new List<Device>();
            devices.Add(new Device { Id = "ST10102180105040", MAC = "ED:12:EB:D4:39:AF", Model = "ST101", RFID = "480A2B438A", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105041", MAC = "D1:32:B8:F3:D7:6C", Model = "ST101", RFID = "GK7HODNOTH", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105042", MAC = "F5:2E:25:0F:FA:4F", Model = "ST101", RFID = "PX8ESOMD77", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105043", MAC = "FC:25:B4:06:45:38", Model = "ST101", RFID = "STGQBTDI4J", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105044", MAC = "E0:DA:F6:F3:1D:BF", Model = "ST101", RFID = "9AR3UP8XAD", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105045", MAC = "D2:95:3C:2B:B9:30", Model = "ST101", RFID = "E1I9DO61UU", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105046", MAC = "EE:C8:BE:2D:6A:91", Model = "ST101", RFID = "4L212ILPCF", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105047", MAC = "D4:F5:8A:BD:DF:FE", Model = "ST101", RFID = "PP6Z0I0P8R", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105048", MAC = "F7:73:5D:16:FE:BF", Model = "ST101", RFID = "MYM4E6MNZB", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105049", MAC = "D0:BF:DF:6F:33:4A", Model = "ST101", RFID = "1L6Z7RXB2U", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105050", MAC = "CC:E0:7A:83:C3:C7", Model = "ST101", RFID = "GU1MQV5WLL", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105051", MAC = "D7:57:41:9F:15:A0", Model = "ST101", RFID = "W9W2PT74FI", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105052", MAC = "D7:2E:E1:F0:9A:B0", Model = "ST101", RFID = "K24QM8VTE3", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105053", MAC = "F1:68:F0:40:17:0F", Model = "ST101", RFID = "HDT7376TE8", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105054", MAC = "C4:31:9A:CF:F8:8B", Model = "ST101", RFID = "8AEG2OULQX", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105055", MAC = "C3:CE:0B:20:E0:79", Model = "ST101", RFID = "J97SBXZHJA", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105056", MAC = "FB:05:C9:C9:AC:17", Model = "ST101", RFID = "6X5N0X8IHP", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105057", MAC = "FD:FC:6A:94:5A:0E", Model = "ST101", RFID = "3OXZCSJ02C", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105058", MAC = "F5:7C:69:8A:A4:BC", Model = "ST101", RFID = "7ZJT44FYUG", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105059", MAC = "CD:FE:91:33:81:22", Model = "ST101", RFID = "C3QE8B6KX7", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105060", MAC = "C4:06:51:93:07:1F", Model = "ST101", RFID = "JQ2T6MT4P7", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105061", MAC = "EA:31:F6:56:ED:13", Model = "ST101", RFID = "YRY9N2PU76", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105062", MAC = "D8:8B:6E:8D:31:6C", Model = "ST101", RFID = "8L6EC8X1MM", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105063", MAC = "FC:32:0D:E4:CE:51", Model = "ST101", RFID = "YELR83CHIU", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105064", MAC = "E9:31:78:03:C8:75", Model = "ST101", RFID = "TJ7CZE1KWG", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105065", MAC = "D3:01:FB:96:E5:E3", Model = "ST101", RFID = "R6A0X73WAH", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105066", MAC = "F7:14:09:19:6E:0A", Model = "ST101", RFID = "3D3U15XYUD", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105067", MAC = "D4:1B:57:7D:01:7F", Model = "ST101", RFID = "OL8ZB9UUS1", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            devices.Add(new Device { Id = "ST10102180105068", MAC = "D8:B6:17:45:71:4C", Model = "ST101", RFID = "D0CY4JAZT8", Battery = 0, Firmware = null, LastSynced = null, EmployeeId = null, PrimaryAccountId = null, MasterPassword = null, ImportedAt = DateTime.Now });
            //await _context.AddRangeAsync(devices);
            // Add positions
            var positions = new List<Position>();
            positions.Add(new Position { Id = "0", Name = "Chief Executive Officer(CEO)" });
            positions.Add(new Position { Id = "1", Name = "Chief Financial Officer(CFO)" });
            positions.Add(new Position { Id = "2", Name = "Chief Information Officer(CIO)" });
            positions.Add(new Position { Id = "3", Name = "Chief Operating Officer(COO)" });
            positions.Add(new Position { Id = "4", Name = "Chief Security Officer(CSO)" });
            positions.Add(new Position { Id = "5", Name = "Chief Technology Officer(CTO)" });
            positions.Add(new Position { Id = "6", Name = "Cloud Architect" });
            positions.Add(new Position { Id = "7", Name = "Configuration Manager" });
            positions.Add(new Position { Id = "8", Name = "Content Manager" });
            positions.Add(new Position { Id = "9", Name = "Contracts Manager" });
            positions.Add(new Position { Id = "10", Name = "Data Analyst" });
            positions.Add(new Position { Id = "11", Name = "Data Architect" });
            positions.Add(new Position { Id = "12", Name = "Data Center Manager" });
            positions.Add(new Position { Id = "13", Name = "Data Center System Administrator" });
            positions.Add(new Position { Id = "14", Name = "Data Migration Architect" });
            positions.Add(new Position { Id = "15", Name = "Data Scientist" });
            positions.Add(new Position { Id = "16", Name = "Database Administrator(DBA)" });
            positions.Add(new Position { Id = "17", Name = "Database Analyst" });
            positions.Add(new Position { Id = "18", Name = "Database Developer" });
            positions.Add(new Position { Id = "19", Name = "Designer" });
            positions.Add(new Position { Id = "20", Name = "Desktop Support Technician" });
            positions.Add(new Position { Id = "21", Name = "Developer" });
            positions.Add(new Position { Id = "22", Name = "Director of Delivery" });
            positions.Add(new Position { Id = "23", Name = "Director of Information Systems" });
            positions.Add(new Position { Id = "24", Name = "Director of Operations" });
            positions.Add(new Position { Id = "25", Name = "Disaster Recovery Specialist" });
            positions.Add(new Position { Id = "26", Name = "Enterprise Solutions Architect" });
            positions.Add(new Position { Id = "27", Name = "Enterprise Architect" });
            positions.Add(new Position { Id = "28", Name = "ERP Analyst" });
            positions.Add(new Position { Id = "29", Name = "Game Architect" });
            positions.Add(new Position { Id = "30", Name = "Game Designer" });
            positions.Add(new Position { Id = "31", Name = "Game Developer" });
            positions.Add(new Position { Id = "32", Name = "Graphic Designer" });
            positions.Add(new Position { Id = "33", Name = "Help Desk Operator" });
            positions.Add(new Position { Id = "34", Name = "HR" });
            positions.Add(new Position { Id = "35", Name = "HR Manager" });
            positions.Add(new Position { Id = "36", Name = "Information Architect" });
            positions.Add(new Position { Id = "37", Name = "Information Assurance Engineer" });
            positions.Add(new Position { Id = "38", Name = "Information Security Architect" });
            positions.Add(new Position { Id = "39", Name = "Information Technology Director" });
            positions.Add(new Position { Id = "40", Name = "Infrastructure Manager" });
            positions.Add(new Position { Id = "41", Name = "Integration Architect" });
            positions.Add(new Position { Id = "42", Name = "iOS Developer" });
            positions.Add(new Position { Id = "43", Name = "IT Auditor" });
            positions.Add(new Position { Id = "44", Name = "Agile Business Analyst" });
            await _context.AddRangeAsync(positions);
            // Add companies
            var comanies = new List<Company>();
            comanies.Add(new Company { Id = "0", Name = "Hideez" });
            comanies.Add(new Company { Id = "1", Name = "Google" });
            await _context.AddRangeAsync(comanies);
            // Add departments
            var departments = new List<Department>();
            departments.Add(new Department { Id = "0", CompanyId = "0", Name = "Development" });
            departments.Add(new Department { Id = "1", CompanyId = "0", Name = "Financial" });
            departments.Add(new Department { Id = "2", CompanyId = "0", Name = "HR" });
            departments.Add(new Department { Id = "3", CompanyId = "0", Name = "Legal" });
            departments.Add(new Department { Id = "4", CompanyId = "0", Name = "Management" });
            departments.Add(new Department { Id = "5", CompanyId = "0", Name = "Marketing" });
            departments.Add(new Department { Id = "6", CompanyId = "1", Name = "PR" });
            departments.Add(new Department { Id = "7", CompanyId = "1", Name = "Production" });
            departments.Add(new Department { Id = "8", CompanyId = "1", Name = "Sales" });
            departments.Add(new Department { Id = "9", CompanyId = "1", Name = "Security" });
            departments.Add(new Department { Id = "10", CompanyId = "1", Name = "Support" });
            departments.Add(new Department { Id = "11", CompanyId = "1", Name = "QA" });
            await _context.AddRangeAsync(departments);
            // Add templates
            var templates = new List<Template>();
            templates.Add(new Template { Id = Guid.NewGuid().ToString(), Name = "Google drive", Urls = "drive.google.com", Apps = null, Deleted = false });
            templates.Add(new Template { Id = Guid.NewGuid().ToString(), Name = "Google mail", Urls = "mail.google.com", Apps = null, Deleted = false });
            templates.Add(new Template { Id = Guid.NewGuid().ToString(), Name = "Google photo", Urls = "photos.google.com", Apps = null, Deleted = false });
            templates.Add(new Template { Id = Guid.NewGuid().ToString(), Name = "Stackoverflow", Urls = "stackoverflow.com", Apps = null, Deleted = false });
            templates.Add(new Template { Id = Guid.NewGuid().ToString(), Name = "Github", Urls = "github.com", Apps = null, Deleted = false });
            templates.Add(new Template { Id = Guid.NewGuid().ToString(), Name = "Skype", Urls = null, Apps = "Skype", Deleted = false });
            templates.Add(new Template { Id = Guid.NewGuid().ToString(), Name = "Figma", Urls = null, Apps = "Figma", Deleted = false });
            templates.Add(new Template { Id = Guid.NewGuid().ToString(), Name = "Google accounts", Urls = "drive.google.com;mail.google.com;photos.google.com", Apps = null, Deleted = false });
            templates.Add(new Template { Id = Guid.NewGuid().ToString(), Name = "Telegram", Urls = null, Apps = "Telegram Desktop", Deleted = false });
            await _context.AddRangeAsync(templates);
            // Add sharedAccounts
            var sharedAccounts = new List<SharedAccount>();
            sharedAccounts.Add(new SharedAccount { Id = Guid.NewGuid().ToString(), Name = "Microsoft", Urls = "microsoft.com", Apps = null, Login = "MyCorp", Password = "password", PasswordChangedAt = DateTime.Now, OtpSecret = "otp", OtpSecretChangedAt = DateTime.Now, Deleted = false });
            sharedAccounts.Add(new SharedAccount { Id = Guid.NewGuid().ToString(), Name = "Wikipedia", Urls = "wikipedia.org", Apps = null, Login = "Wiki", Password = "password", PasswordChangedAt = DateTime.Now, OtpSecret = "otp", OtpSecretChangedAt = DateTime.Now, Deleted = false });
            sharedAccounts.Add(new SharedAccount { Id = Guid.NewGuid().ToString(), Name = "Google", Urls = "google.com", Apps = null, Login = "Managers", Password = "password", PasswordChangedAt = DateTime.Now, OtpSecret = "otp", OtpSecretChangedAt = DateTime.Now, Deleted = false });
            sharedAccounts.Add(new SharedAccount { Id = Guid.NewGuid().ToString(), Name = "Github", Urls = "github.com", Apps = null, Login = "CorpDev", Password = "password", PasswordChangedAt = DateTime.Now, OtpSecret = "otp", OtpSecretChangedAt = DateTime.Now, Deleted = false });
            sharedAccounts.Add(new SharedAccount { Id = Guid.NewGuid().ToString(), Name = "Skype", Urls = null, Apps = "Skype", Login = "Corp", Password = "password", PasswordChangedAt = DateTime.Now, OtpSecret = "otp", OtpSecretChangedAt = DateTime.Now, Deleted = false });
            sharedAccounts.Add(new SharedAccount { Id = Guid.NewGuid().ToString(), Name = "ICQ", Urls = null, Apps = "ICQ", Login = "Shared", Password = "password", PasswordChangedAt = DateTime.Now, OtpSecret = "otp", OtpSecretChangedAt = DateTime.Now, Deleted = false });
            await _context.AddRangeAsync(sharedAccounts);
            // Add employees
            var employees = new List<Employee>();
            employees.Add(new Employee { Id = Guid.NewGuid().ToString(), FirstName = "Kenney", LastName = "Blankenship", Email = "k_blankenship@bellsouth.net", PhoneNumber = "(432)745-9869", DepartmentId = "1", PositionId = "12" });
            employees.Add(new Employee { Id = Guid.NewGuid().ToString(), FirstName = "Rahid", LastName = "Araufi", Email = "rachidarroufi@yahoo.comt", PhoneNumber = "(232)259-7752", DepartmentId = "2", PositionId = "1" });
            employees.Add(new Employee { Id = Guid.NewGuid().ToString(), FirstName = "Jayme", LastName = "Haney", Email = "jayme76148@yahoo.com", PhoneNumber = "(222)129-4857", DepartmentId = "3", PositionId = "5" });
            employees.Add(new Employee { Id = Guid.NewGuid().ToString(), FirstName = "Robert", LastName = "Glassman", Email = "rsglassman@icloud.com", PhoneNumber = "(432)878-1236", DepartmentId = "4", PositionId = "10" });
            employees.Add(new Employee { Id = Guid.NewGuid().ToString(), FirstName = "James", LastName = "Bishop", Email = "j_bishop@bellsouth.net", PhoneNumber = "(252)728-6428", DepartmentId = "5", PositionId = "20" });
            employees.Add(new Employee { Id = Guid.NewGuid().ToString(), FirstName = "Uri", LastName = "Commoro", Email = "commorouri@gmail.com", PhoneNumber = "(333)252-6845", DepartmentId = "6", PositionId = "18" });
            employees.Add(new Employee { Id = Guid.NewGuid().ToString(), FirstName = "Hosie", LastName = "Harris", Email = "hoharris@ultimatemedical.edu", PhoneNumber = "(948)111-7752", DepartmentId = "7", PositionId = "30" });
            employees.Add(new Employee { Id = Guid.NewGuid().ToString(), FirstName = "Billy", LastName = "Kissenger", Email = "billy_bons@bellsouth.net", PhoneNumber = "(432)736-2896", DepartmentId = "8", PositionId = "34" });
            await _context.AddRangeAsync(employees);
            // Save all
            await _context.SaveChangesAsync();
        }
    }
}