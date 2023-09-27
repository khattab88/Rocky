using Rocky.Data.Repositories.Interfaces;
using Rocky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Data.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context) : base(context) 
        {
        }

        public void Update(Category category)
        {
            var existing = base.FirstOrDefault(c => c.Id == category.Id);

            if(existing != null) 
            {
                existing.Name = category.Name;
                existing.DisplayOrder = category.DisplayOrder;
            }
        }
    }
}
