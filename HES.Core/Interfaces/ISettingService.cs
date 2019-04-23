using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HES.Core.Interfaces
{
    public interface ISettingsService
    {
        IQueryable<Department> DepartmentQuery();
    }
}
