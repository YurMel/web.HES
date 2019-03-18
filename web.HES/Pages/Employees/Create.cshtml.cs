using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using web.HES.Data;

namespace web.HES.Pages.Employees
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        [BindProperty]
        public string SelectedDeviceId { get; set; }

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["CompanyId"] = new SelectList(_context.Set<Company>(), "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(_context.Set<Department>(), "Id", "Name");
            ViewData["PositionId"] = new SelectList(_context.Set<Position>(), "Id", "Name");
            ViewData["DeviceId"] = new SelectList(_context.Set<Device>().Where(d => d.EmployeeId == null), "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Employee Employee { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Add employee
            _context.Employee.Add(Employee);
            await _context.SaveChangesAsync();

            // Add devive to employee
            var device = _context.Devices.FirstOrDefault(d => d.Id == SelectedDeviceId);
            if (device != null)
            {
                device.EmployeeId = Employee.Id;
                _context.Devices.Update(device);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}