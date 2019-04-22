using HES.Core.Entities;
using HES.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly IAsyncRepository<Template> _templateRepository;

        public TemplateService(IAsyncRepository<Template> repository)
        {
            _templateRepository = repository;
        }

        public async Task<IList<Template>> GetAllAsync()
        {
            return await _templateRepository.GetAllAsync();
        }

        public async Task<IList<Template>> GetAllWhereAsync(Expression<Func<Template, bool>> predicate)
        {
            return await _templateRepository.GetAllWhereAsync(predicate);
        }

        public async Task<IList<Template>> GetAllIncludeAsync(params Expression<Func<Template, object>>[] navigationProperties)
        {
            return await _templateRepository.GetAllIncludeAsync(navigationProperties);
        }

        public async Task<Template> GetFirstOrDefaulAsync()
        {
            return await _templateRepository.GetFirstOrDefaulAsync();
        }

        public async Task<Template> GetFirstOrDefaulAsync(Expression<Func<Template, bool>> match)
        {
            return await _templateRepository.GetFirstOrDefaulAsync(match);
        }

        public async Task<Template> GetFirstOrDefaulIncludeAsync(Expression<Func<Template, bool>> where, params Expression<Func<Template, object>>[] navigationProperties)
        {
            return await _templateRepository.GetFirstOrDefaulIncludeAsync(where, navigationProperties);
        }

        public async Task<Template> GetByIdAsync(dynamic id)
        {
            return await _templateRepository.GetByIdAsync(id);
        }

        public async Task<Template> CreateTmplateAsync(Template template)
        {
            if (template == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            return await _templateRepository.AddAsync(template);
        }

        public async Task EditTemplateAsync(Template template)
        {
            if (template == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            await _templateRepository.UpdateAsync(template);
        }

        public async Task DeleteTemplateAsync(string id)
        {
            if (id == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var template = await _templateRepository.GetByIdAsync(id);
            if (template == null)
            {
                throw new Exception("Template does not exist.");
            }
            await _templateRepository.DeleteAsync(template);
        }

        public bool Exist(Expression<Func<Template, bool>> predicate)
        {
            return _templateRepository.Exist(predicate);
        }
    }
}