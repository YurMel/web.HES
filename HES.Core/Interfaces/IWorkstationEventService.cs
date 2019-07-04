﻿using HES.Core.Entities;
using System.Linq;

namespace HES.Core.Interfaces
{
    public interface IWorkstationEventService
    {
        IQueryable<WorkstationEvent> WorkstationEventQuery();
        IQueryable<Workstation> WorkstationQuery();
        IQueryable<Device> DeviceQuery();
        IQueryable<Employee> EmployeeQuery();
        IQueryable<Company> CompanyQuery();
        IQueryable<Department> DepartmentQuery();
        IQueryable<DeviceAccount> DeviceAccountQuery();
    }
}