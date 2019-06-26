using HES.Core.Entities;
using HES.Core.Interfaces;
using System.Linq;

namespace HES.Core.Services
{
    public class WorkstationSessionService : IWorkstationSessionService
    {
        private readonly IAsyncRepository<WorkstationSession> _workstationSessionRepository;

        public WorkstationSessionService(IAsyncRepository<WorkstationSession> workstationSessionRepository)
        {
            _workstationSessionRepository = workstationSessionRepository;
        }

        public IQueryable<WorkstationSession> WorkstationSessionQuery()
        {
            return _workstationSessionRepository.Query();
        }
    }
}