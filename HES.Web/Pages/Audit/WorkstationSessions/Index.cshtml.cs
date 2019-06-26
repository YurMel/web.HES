using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSessions
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationSessionService _workstationSessionService;
        public IList<WorkstationSession> WorkstationSessions { get; set; }

        public IndexModel(IWorkstationSessionService workstationSessionService)
        {
            _workstationSessionService = workstationSessionService;
        }

        public async Task OnGetAsync()
        {
            WorkstationSessions = await _workstationSessionService
                .WorkstationSessionQuery()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .OrderBy(w => w.StartTime)
                .ToListAsync();
        }
    }
}