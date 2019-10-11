using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Dashboard
{
    public class WorkstationsOnlineModel : PageModel
    {
        private readonly IWorkstationService _workstationService;

        public IList<Workstation> Workstations { get; set; }

        public WorkstationsOnlineModel(IWorkstationService workstationService)
        {
            _workstationService = workstationService;
        }

        public async Task OnGet()
        {
            var all = await _workstationService.Query().Include(w => w.Department.Company).ToListAsync();
            Workstations = all.Where(w => w.IsOnline == true).ToList();
        }
    }
}