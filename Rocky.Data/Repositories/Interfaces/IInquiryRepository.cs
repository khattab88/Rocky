using Rocky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Data.Repositories.Interfaces
{
    public interface IInquiryRepository : IRepository<Inquiry>
    {
        void Update(Inquiry inquiry);
    }
}
