using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationEvents
{
    public class IndexModel : PageModel
    {
        private readonly IWorkstationEventService _workstationEventService;
        public IList<WorkstationEvent> WorkstationEvents { get; set; }

        public IndexModel(IWorkstationEventService workstationEventService)
        {
            _workstationEventService = workstationEventService;
        }
        
        public async Task OnGetAsync()
        {
            WorkstationEvents = await _workstationEventService
                .WorkstationEventQuery()
                .Include(w => w.Workstation)
                .Include(w => w.Device)
                .Include(w => w.Employee)
                .Include(w => w.Department.Company)
                .Include(w => w.DeviceAccount)
                .OrderBy(w => w.Date)
                .ToListAsync();
        }
    }
}