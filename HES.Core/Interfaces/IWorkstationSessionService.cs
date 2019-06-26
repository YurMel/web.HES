using HES.Core.Entities;
using System.Linq;

namespace HES.Core.Interfaces
{
    public interface IWorkstationSessionService
    {
        IQueryable<WorkstationSession> WorkstationSessionQuery();
    }
}