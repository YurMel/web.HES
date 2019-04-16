using HES.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ITemplateService
    {
        Task<IList<Template>> GetAllAsync();
        Task<IList<Template>> GetAllWhereAsync(Expression<Func<Template, bool>> predicate);
        Task<IList<Template>> GetAllIncludeAsync(params Expression<Func<Template, object>>[] navigationProperties);
        Task<Template> GetFirstOrDefaulAsync();
        Task<Template> GetFirstOrDefaulAsync(Expression<Func<Template, bool>> match);
        Task<Template> GetFirstOrDefaulIncludeAsync(Expression<Func<Template, bool>> where, params Expression<Func<Template, object>>[] navigationProperties);
        Task<Template> GetByIdAsync(dynamic id);
        Task<Template> AddAsync(Template entity);
        Task<IList<Template>> AddRangeAsync(IList<Template> entity);
        Task UpdateAsync(Template entity);
        Task UpdateOnlyPropAsync(Template entity, string[] properties);
        Task DeleteAsync(Template entity);
        bool Exist(Expression<Func<Template, bool>> predicate);
    }
}