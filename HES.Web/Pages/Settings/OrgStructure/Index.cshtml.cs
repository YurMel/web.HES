using HES.Core.Entities;
using HES.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using HES.Web.Data;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IList<Company> Companies { get; set; }
        public IList<Department> Departments { get; set; }

        public Company Company { get; set; }
        public Department Department { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Companies = await _context.Companies.ToListAsync();
            Departments = await _context.Departments.Include(d => d.Company).ToListAsync();
        }

        #region Company

        public IActionResult OnGetCreateCompany()
        {
            return Partial("_CreateCompany", this);
        }

        public async Task<IActionResult> OnPostCreateCompanyAsync(Company Company)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            _context.Companies.Add(Company);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditCompanyAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Company = await _context.Companies.FirstOrDefaultAsync(m => m.Id == id);

            if (Company == null)
            {
                return NotFound();
            }

            return Partial("_EditCompany", this);
        }

        public async Task<IActionResult> OnPostEditCompanyAsync(string id, Company Company)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            Company.Id = id;
            _context.Attach(Company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(Company.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool CompanyExists(string id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }

        public async Task<IActionResult> OnGetDeleteCompanyAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Company = await _context.Companies.FirstOrDefaultAsync(m => m.Id == id);

            if (Company == null)
            {
                return NotFound();
            }

            return Partial("_DeleteCompany", this);
        }

        public async Task<IActionResult> OnPostDeleteCompanyAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Company = await _context.Companies.FindAsync(id);

            if (Company != null)
            {
                _context.Companies.Remove(Company);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }

        #endregion

        #region Department

        public IActionResult OnGetCreateDepartment()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            return Partial("_CreateDepartment", this);
        }

        public async Task<IActionResult> OnPostCreateDepartmentAsync(Department Department)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            _context.Departments.Add(Department);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditDepartmentAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Department = await _context.Departments
                .Include(d => d.Company).FirstOrDefaultAsync(m => m.Id == id);

            if (Department == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            return Partial("_EditDepartment", this);
        }

        public async Task<IActionResult> OnPostEditDepartmentAsync(string id, Department Department)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            Department.Id = id;
            _context.Attach(Department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(Department.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool DepartmentExists(string id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }

        public async Task<IActionResult> OnGetDeleteDepartmentAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Department = await _context.Departments
                .Include(d => d.Company).FirstOrDefaultAsync(m => m.Id == id);

            if (Department == null)
            {
                return NotFound();
            }
            return Partial("_DeleteDepartment", this);
        }

        public async Task<IActionResult> OnPostDeleteDepartmentAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Department = await _context.Departments.FindAsync(id);

            if (Department != null)
            {
                _context.Departments.Remove(Department);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}