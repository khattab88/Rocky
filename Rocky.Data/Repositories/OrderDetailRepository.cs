using Rocky.Data.Repositories.Interfaces;
using Rocky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Data.Repositories
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void Update(OrderDetail orderDetail)
        {
            _dbSet.Update(orderDetail);
        }
    }
}
