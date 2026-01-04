using Securitas8._8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Data.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        void Create(Order order);
        List<Order> GetOrders();
    }
}
