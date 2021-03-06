﻿using HES.Core.Entities;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ITemplateService
    {
        IQueryable<Template> TemplateQuery();
        Task<Template> TemplateGetByIdAsync(dynamic id);
        Task<Template> CreateTmplateAsync(Template entity);
        Task EditTemplateAsync(Template template);
        Task DeleteTemplateAsync(string id);
        Task<bool> ExistAsync(Expression<Func<Template, bool>> predicate);
    }
}