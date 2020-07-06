using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AuthService.Model;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repositories
{
    public class Repository<T> : IRepository<T>
    where T : BaseEntityToken
    {
        private readonly DbContextOptions<SqlDbContext> _options;

        public Repository(DbContextOptions<SqlDbContext> options)
        {
            _options = options;
        }

        public void Add(T entity)
        {
            using (var db = new SqlDbContext(_options))
            {
                var result = db.Set<T>().FirstOrDefault(x => x.Id == entity.Id);
                if (result != null) return;

                db.Add(entity);
                db.SaveChanges();
            }
        }

        public async Task<T> GetByIdAsync(string id)
        {
            using (var db = new SqlDbContext(_options))
            {
                var result = await db.Set<T>().FirstOrDefaultAsync(x => x.Id == id);

                return result;
            }
        }

        public async Task<List<T>> ListAsync()
        {
            using (var db = new SqlDbContext(_options))
            {
                var result = await db.Set<T>().ToListAsync();

                return result;
            }
        }

        public void Delete(string id)
        {
            using (var db = new SqlDbContext(_options))
            {
                var result = db.Set<T>().FirstOrDefault(x => x.Id == id);
                if (result != null)
                {
                    db.Remove(result);
                    db.SaveChanges();
                }
            }
        }

        public void Update(T entity)
        {
            using (var db = new SqlDbContext(_options))
            {
                db.Update(entity);
                db.SaveChanges();
            }
        }

        public async Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate)
        {
            using (var db = new SqlDbContext(_options))
            {
                var result = await db.Set<T>().Where(predicate).ToListAsync();

                return result;
            }
        }

        public void BulkAdd(IEnumerable<T> entities)
        {
            using (var db = new SqlDbContext(_options))
            {
                db.AddRange(entities);
                db.SaveChanges();
            }
        }
    }
}
