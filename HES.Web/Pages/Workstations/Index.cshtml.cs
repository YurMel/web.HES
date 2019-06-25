using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Computers
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationService _workstationService;
        private readonly ILogger<IndexModel> _logger;

        public IList<Workstation> Workstations { get; set; }
        public Workstation Workstation { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public IndexModel(IWorkstationService workstationService, ILogger<IndexModel> logger)
        {
            _workstationService = workstationService;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            Workstations = await _workstationService.WorkstationQuery()
                .Include(c => c.Company)
                .Include(c => c.Department)
                .ToListAsync();
        }

        public async Task<JsonResult> OnGetJsonDepartmentAsync(string id)
        {
            return new JsonResult(await _workstationService.DepartmentQuery().Where(d => d.CompanyId == id).ToListAsync());
        }

        public async Task<IActionResult> OnGetEditDepartmentAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Workstation = await _workstationService
                .WorkstationQuery()
                .Include(c => c.Company)
                .Include(c => c.Department)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Workstation == null)
            {
                _logger.LogWarning("Computer == null");
                return NotFound();
            }

            ViewData["CompanyId"] = new SelectList(await _workstationService.CompanyQuery().ToListAsync(), "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(await _workstationService.DepartmentQuery().Where(d => d.CompanyId == Workstation.Department.CompanyId).ToListAsync(), "Id", "Name");

            return Partial("_EditDepartment", this);
        }

        public async Task<IActionResult> OnPostEditDepartmentAsync(Workstation Computer)
        {
            if (Computer == null)
            {
                _logger.LogWarning("departmentId == null");
                return RedirectToPage("./Index");
            }

            try
            {
                await _workstationService.EditDepartmentAsync(Computer);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnGetApproveComputerAsync(string id)
        {
            if (id == null)
            {
                _logger.LogWarning("id == null");
                return NotFound();
            }

            Workstation = await _workstationService
               .WorkstationQuery()
               .FirstOrDefaultAsync(c => c.Id == id);

            return Partial("_ApproveComputer", this);
        }

        public async Task<IActionResult> OnPostApproveComputerAsync(string computerId)
        {
            if (computerId == null)
            {
                _logger.LogWarning("computerId == null");
                return RedirectToPage("./Index");
            }
            try
            {
                await _workstationService.ApproveWorkstationAsync(computerId);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                _logger.LogError(ex.Message);
            }

            return RedirectToPage("./Index");
        }
    }
}