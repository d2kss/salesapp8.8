using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Models
{
    public class Order
    {
        public int OrderNo { get; set; }
        public Customer Customer { get; set; } = new();
        public string SalesPerson { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public string Status { get; set; } = "New";
        public List<OrderItem> Items { get; set; } = new();

        public Address Address { get; set; } = new();
        public decimal TotalAmount =>
            Items.Sum(i => i.FinalPrice);
    }

}
