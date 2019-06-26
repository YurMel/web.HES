using HES.Core.Entities;
using HES.Core.Interfaces;
using System.Linq;

namespace HES.Core.Services
{
    public class WorkstationEventService : IWorkstationEventService
    {
        private readonly IAsyncRepository<WorkstationEvent> _workstationEventRepository;

        public WorkstationEventService(IAsyncRepository<WorkstationEvent> workstationEventRepository)
        {
            _workstationEventRepository = workstationEventRepository;
        }

        public IQueryable<WorkstationEvent> WorkstationEventQuery()
        {
            return _workstationEventRepository.Query();
        }
    }
}