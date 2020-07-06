using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AuthService.Model;

namespace AuthService.Repositories
{
    public interface IRepository<T> where T : BaseEntityToken
    {
        Task<T> GetByIdAsync(string id);
        Task<List<T>> ListAsync();
        Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Update(T entity);
        void Delete(string id);
        void BulkAdd(IEnumerable<T> entities);
    }
}
