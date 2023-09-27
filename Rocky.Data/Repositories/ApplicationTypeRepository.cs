using Rocky.Data.Repositories.Interfaces;
using Rocky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Data.Repositories
{
    public class ApplicationTypeRepository : Repository<ApplicationType>, IApplicationTypeRepository
    {
        public ApplicationTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(ApplicationType applicationType)
        {
            var existing = base.FirstOrDefault(a => a.Id == applicationType.Id);

            if (existing != null) 
            {
                existing.Name = applicationType.Name;
            }
        }
    }
}
