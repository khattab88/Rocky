using Rocky.Data.Repositories.Interfaces;
using Rocky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Data.Repositories
{
    public class InquiryDetailRepository : Repository<InquiryDetail>, IInquiryDetailRepository
    {
        public InquiryDetailRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(InquiryDetail inquiryDetail)
        {
            _dbSet.Update(inquiryDetail);
        }
    }
}
