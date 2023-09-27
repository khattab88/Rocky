using Microsoft.EntityFrameworkCore;
using Rocky.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public T Find(int id)
        {
            return _dbSet.Find(id);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> filter = null, string includedProperties = null, bool enableTracking = true)
        {
            IQueryable<T> querable = _dbSet;

            if (filter != null)
            {
                querable = querable.Where(filter);
            }

            if (includedProperties != null)
            {
                foreach (var property in includedProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    querable = querable.Include(property);
                }
            }

            if (!enableTracking)
            {
                querable = querable.AsNoTracking();
            }

            return querable.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includedProperties = null, bool enableTracking = true)
        {
            IQueryable<T> querable = _dbSet;

            if(filter != null) 
            {
                querable = querable.Where(filter);
            }

            if(includedProperties != null) 
            {
                foreach(var property in includedProperties.Split(',', StringSplitOptions.RemoveEmptyEntries)) 
                {
                    querable = querable.Include(property);
                }
            }

            if(orderBy != null)
            {
                querable = orderBy(querable);
            }

            if(!enableTracking) 
            {
                querable = querable.AsNoTracking();
            }

            return querable.ToList();
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}
