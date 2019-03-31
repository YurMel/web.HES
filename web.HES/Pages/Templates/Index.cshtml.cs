using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using web.HES.Data;

namespace web.HES.Pages.Templates
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IList<Template> Templates { get; set; }

        [BindProperty]
        public Template Template { get; set; }

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnGetAsync()
        {
            Templates = await _context.Templates.ToListAsync();
        }

        #region Tempalate

        public IActionResult OnGetCreateTemplate()
        {
            return Partial("_CreateTemplate", this);
        }

        public async Task<IActionResult> OnPostCreateTemplateAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            _context.Templates.Add(Template);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditTemplateAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Template = await _context.Templates.FirstOrDefaultAsync(m => m.Id == id);

            if (Template == null)
            {
                return NotFound();
            }
            return Partial("_EditTemplate", this);
        }

        public async Task<IActionResult> OnPostEditTemplateAsync(string id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            Template.Id = id;
            _context.Attach(Template).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TemplateExists(Template.Id))
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

        private bool TemplateExists(string id)
        {
            return _context.Templates.Any(e => e.Id == id);
        }

        public async Task<IActionResult> OnGetDeleteTemplateAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Template = await _context.Templates.FirstOrDefaultAsync(m => m.Id == id);

            if (Template == null)
            {
                return NotFound();
            }
            return Partial("_DeleteTemplate", this);

        }

        public async Task<IActionResult> OnPostDeleteTemplateAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Template = await _context.Templates.FindAsync(id);

            if (Template != null)
            {
                _context.Templates.Remove(Template);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}
