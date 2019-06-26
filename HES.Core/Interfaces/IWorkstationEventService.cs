using HES.Core.Entities;
using System.Linq;

namespace HES.Core.Interfaces
{
    public interface IWorkstationEventService
    {
        IQueryable<WorkstationEvent> WorkstationEventQuery();
    }
}