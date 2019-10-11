using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Entities;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HES.Web.Pages.Dashboard
{
    public class OpenedSessionsModel : PageModel
    {
        private readonly IWorkstationSessionService _workstationSessionService;

        public IList<WorkstationSession> WorkstationSessions { get; set; }

        public OpenedSessionsModel(IWorkstationSessionService workstationSessionService)
        {
            _workstationSessionService = workstationSessionService;
        }

        public async Task OnGet()
        {
            WorkstationSessions = await _workstationSessionService.GetOpenedSessionsAsync();
        }
    }
}