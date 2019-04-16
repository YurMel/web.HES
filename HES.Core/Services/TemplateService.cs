using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly IAsyncRepository<Template> _repository;

        public TemplateService(IAsyncRepository<Template> repository)
        {
            _repository = repository;
        }

        public async Task<IList<Template>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IList<Template>> GetAllWhereAsync(Expression<Func<Template, bool>> predicate)
        {
            return await _repository.GetAllWhereAsync(predicate);
        }

        public async Task<IList<Template>> GetAllIncludeAsync(params Expression<Func<Template, object>>[] navigationProperties)
        {
            return await _repository.GetAllIncludeAsync(navigationProperties);
        }

        public async Task<Template> GetFirstOrDefaulAsync()
        {
            return await _repository.GetFirstOrDefaulAsync();
        }

        public async Task<Template> GetFirstOrDefaulAsync(Expression<Func<Template, bool>> match)
        {
            return await _repository.GetFirstOrDefaulAsync(match);
        }

        public async Task<Template> GetFirstOrDefaulIncludeAsync(Expression<Func<Template, bool>> where, params Expression<Func<Template, object>>[] navigationProperties)
        {
            return await _repository.GetFirstOrDefaulIncludeAsync(where, navigationProperties);
        }

        public async Task<Template> GetByIdAsync(dynamic id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Template> AddAsync(Template entity)
        {
            return await _repository.AddAsync(entity);
        }

        public async Task<IList<Template>> AddRangeAsync(IList<Template> entity)
        {
            return await _repository.AddRangeAsync(entity);
        }

        public async Task UpdateAsync(Template entity)
        {
            await _repository.UpdateAsync(entity);
        }

        public async Task UpdateOnlyPropAsync(Template entity, string[] properties)
        {
            await _repository.UpdateOnlyPropAsync(entity, properties);
        }

        public async Task DeleteAsync(Template entity)
        {
            await _repository.DeleteAsync(entity);
        }

        public bool Exist(Expression<Func<Template, bool>> predicate)
        {
            return _repository.Exist(predicate);
        }       
    }
}