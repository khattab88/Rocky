using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Data.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        T Find(int id);

        IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includedProperties = null,
            bool enableTracking = true);

        T FirstOrDefault(
            Expression<Func<T, bool>> filter = null,
            string includedProperties = null,
            bool enableTracking = true);

        void Add(T entity);

        void Remove(T entity);

        void RemoveRange(IEnumerable<T> entities);

        void SaveChanges();
    }
}
