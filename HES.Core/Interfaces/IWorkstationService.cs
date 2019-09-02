using HES.Core.Entities;
using Hideez.SDK.Communication.HES.Client;
using Hideez.SDK.Communication.Workstation;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IWorkstationService
    {
        IQueryable<Workstation> WorkstationQuery();
        IQueryable<WorkstationBinding> WorkstationBindingQuery();
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        Task<bool> ExistAsync(Expression<Func<Workstation, bool>> predicate);
        Task AddWorkstationAsync(WorkstationInfo workstationInfo);
        Task UpdateWorkstationInfoAsync(WorkstationInfo workstationInfo);
        Task EditDepartmentAsync(Workstation workstation);
        Task ApproveWorkstationAsync(string id);
        Task UnapproveWorkstationAsync(string id);
        Task AddBindingAsync(string workstationId, bool allowRfid, bool allowBleTap, bool allowProximity, string[] selectedDevices);
        Task AddMultipleBindingAsync(string[] workstationsId, bool allowRfid, bool allowBleTap, bool allowProximity, string[] devicesId);
        Task EditBindingAsync(WorkstationBinding workstationBinding);
        Task DeleteBindingAsync(string workstationBindingId);
        Task<UnlockerSettingsInfo> GetWorkstationUnlockerSettingsInfoAsync(string workstationId);
    }
}