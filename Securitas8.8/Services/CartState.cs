using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Services
{
    public class CartState
    {
        public bool ShowAddedAlert { get; private set; }
        public string Message { get; private set; } = "";

        public void ProductAdded(string productName, int servicesCount)
        {
            ShowAddedAlert = true;
            Message = $"{productName} and {servicesCount} service(s) added to cart.";
        }

        public void Clear()
        {
            ShowAddedAlert = false;
            Message = "";
        }
    }
}
