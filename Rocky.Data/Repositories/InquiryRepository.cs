using Rocky.Data.Repositories.Interfaces;
using Rocky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Data.Repositories
{
    public class InquiryRepository : Repository<Inquiry>, IInquiryRepository
    {
        public InquiryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(Inquiry inquiry)
        {
            base._dbSet.Update(inquiry);
        }
    }
}
