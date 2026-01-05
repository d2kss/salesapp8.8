using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Models
{
    public class OrderItem
    {
        public Product Product { get; set; } = new();
        public int Quantity { get; set; }
        public decimal DiscountPercent { get; set; }
        public List<AddOnService> SelectedAddOns { get; set; } = new();
        public List<AddOnService> RequiredMaterials { get; set; } = new();
        public List<AddOnService> MonitoringServices { get; set; } = new();
        public Dictionary<int, int> ServiceQuantities { get; set; } = new();
        public decimal ProductTotal => Product.Price * Quantity;
        public decimal DiscountAmount => ProductTotal * (DiscountPercent / 100);

        public decimal ProductFinalCost => ProductTotal - DiscountAmount;
        public decimal AddOnTotal => SelectedAddOns.Sum(a => a.Price) * Quantity;
        public int GetServiceQty(int serviceId)
    => ServiceQuantities.TryGetValue(serviceId, out var q) ? q : 1;
        public decimal RequiredMaterialTotal =>
        RequiredMaterials.Sum(r =>
            r.Price * GetServiceQty(r.Id)
        );

        public decimal MonitoringTotal =>
            MonitoringServices.Sum(m =>
                m.Price * GetServiceQty(m.Id)
            );
        public decimal FinalPrice => ProductTotal - DiscountAmount + AddOnTotal + RequiredMaterialTotal
        + MonitoringTotal;
        //public decimal FinalPrice =>
        //    (Product.Price * Quantity) -
        //    ((Product.Price * Quantity) * DiscountPercent / 100);
    }
}
