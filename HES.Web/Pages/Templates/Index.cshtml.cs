using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmartBreadcrumbs.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    [Breadcrumb("Templates")]
    public class IndexModel : PageModel
    {
        private readonly ITemplateService _templateService;

        public IList<Template> Templates { get; set; }

        [BindProperty]
        public Template Template { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        public async Task OnGetAsync()
        {
            Templates = await _templateService.TemplateQuery().ToListAsync();
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

            try
            {
                await _templateService.CreateTmplateAsync(Template);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetEditTemplateAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Template = await _templateService
                .TemplateQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Template == null)
            {
                return NotFound();
            }

            return Partial("_EditTemplate", this);
        }

        public async Task<IActionResult> OnPostEditTemplateAsync()
        {
            if (!ModelState.IsValid)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                await _templateService.EditTemplateAsync(Template);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetDeleteTemplateAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Template = await _templateService
                .TemplateQuery()
                .FirstOrDefaultAsync(m => m.Id == id);

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

            try
            {
                await _templateService.DeleteTemplateAsync(id);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }

            return RedirectToPage("./Index");
        }

        #endregion
    }
}