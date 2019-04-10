using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IAsyncRepository<T> where T : class
    {
        Task<IList<T>> GetAllAsync();
        Task<IList<T>> GetAllWhereAsync(Expression<Func<T, bool>> predicate);
        Task<IList<T>> GetAllIncludeAsync(params Expression<Func<T, object>>[] navigationProperties);
        Task<T> GetByIdAsync(int id);
        Task<T> AddAsync(T entity);
        Task<IList<T>> AddRangeAsync(IList<T> entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}