using Securitas8._8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Data.Repositories.Interfaces
{
    public interface IProductRepository
    {
        List<Product> GetAll();
        void Insert(Product product);
    }
}
