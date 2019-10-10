using HES.Core.Entities;
using HES.Core.Entities.Models;
using Hideez.SDK.Communication.HES.DTO;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationSessionService
    {
        IQueryable<WorkstationSession> Query();
        IQueryable<SummaryByDayAndEmployee> SummaryByDayAndEmployeeSqlQuery(string sql);
        IQueryable<SummaryByEmployees> SummaryByEmployeesSqlQuery(string sql);
        IQueryable<SummaryByDepartments> SummaryByDepartmentsSqlQuery(string sql);
        IQueryable<SummaryByWorkstations> SummaryByWorkstationsSqlQuery(string sql);
        Task<int> GetOpenedSessionsCount();
        Task AddOrUpdateWorkstationSession(WorkstationEventDto workstationEventDto);
        Task CloseSessionAsync(string workstationId);
    }
}