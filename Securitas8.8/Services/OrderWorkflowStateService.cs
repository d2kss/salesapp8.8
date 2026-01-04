using Securitas8._8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Services
{
    public enum OrderStep
    {
        CustomerInfo = 1,
        Products = 2,
        Summary = 3
    }
    public class OrderWorkflowStateService
    {
        public Order Order { get; private set; } = CreateNewOrder();
        public OrderStep CurrentStep { get; private set; } = OrderStep.CustomerInfo;
        public void Next()
        {
            if (CurrentStep < OrderStep.Summary)
                CurrentStep++;
        }

        public void Back()
        {
            if (CurrentStep > OrderStep.CustomerInfo)
                CurrentStep--;
        }

        public int ProgressPercent =>
            (int)(((int)CurrentStep - 1) / 2.0 * 100);
        private static Order CreateNewOrder() =>
       new()
       {
           SalesPerson = "Sales User",
           Status = "New"
       };

        public List<Product> Products { get; set; } = new();
        public Dictionary<int, int> Quantities { get; set; } = new();
        public Dictionary<int, decimal> Discounts { get; set; } = new();
        public Dictionary<int, HashSet<int>> SelectedAddOns { get; set; } = new();
        public Dictionary<int, HashSet<int>> SelectedMonitoringServices { get; set; } = new();
        public Dictionary<int, decimal> SurfaceByProduct { get; } = new();
        public decimal SurfaceAreaSqm { get; set; } = 0;
        public Dictionary<(int productId, int materialId), int> RequiredMaterialQty { get; } = new();
        public event Action? OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();
        private readonly HashSet<int> AddedProducts = new();
        public decimal GetSurface(int productId)
        {
            return SurfaceByProduct.TryGetValue(productId, out var v) ? v : 0;
        }

        public void SetSurface(int productId, decimal value)
        {
            SurfaceByProduct[productId] = value;
        }
        public void SetSurfaceArea(decimal sqm)
        {
            SurfaceAreaSqm = Math.Max(0, sqm);
        }
        public int GetRequiredQty(int productId, int materialId, int minQty)
        {
            return RequiredMaterialQty.TryGetValue((productId, materialId), out var v)
                ? Math.Max(v, minQty)
                : minQty;
        }


        public void SetRequiredQty(int productId, int materialId, int qty, int minQty)
        {
            RequiredMaterialQty[(productId, materialId)] = Math.Max(qty, minQty);
            NotifyStateChanged();
        }
        public void SaveCustomer(Customer customer)
        {
            Order.Customer = customer;
        }

        public void AddOrUpdateProduct(
            Product product,
            int quantity,
            decimal discount,
            IEnumerable<AddOnService> addOns,
            IEnumerable<AddOnService> requiredMaterials,
            IEnumerable<AddOnService> monitoringServices)
        {
            var item = Order.Items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (item == null)
            {
                item = new OrderItem { Product = product };
                Order.Items.Add(item);
            }
            if (item != null)
            {
                foreach (var r in item.RequiredMaterials)
                {
                    item.ServiceQuantities[r.Id] =
                      GetRequiredQty(product.Id, r.Id, r.MinQuantity);
                }


                foreach (var a in item.SelectedAddOns)
                {
                    item.ServiceQuantities[a.Id] = item.Quantity;
                }


                foreach (var m in item.MonitoringServices)
                {
                    item.ServiceQuantities[m.Id] = 1;
                }





                item.Quantity = quantity;
                item.DiscountPercent = discount;
                item.SelectedAddOns = addOns.ToList();
                item.RequiredMaterials = requiredMaterials.ToList();
                item.MonitoringServices = monitoringServices.ToList();
            }
            NotifyStateChanged();
        }
        public int GetQuantity(int productId)
        {
            if (!Quantities.ContainsKey(productId))
                Quantities[productId] = 1;

            return Quantities[productId];
        }

        public void SetQuantity(int productId, int value)
        {
            Quantities[productId] = Math.Max(1, value);
            NotifyStateChanged();
        }

        public decimal GetDiscount(int productId)
        {
            if (!Discounts.ContainsKey(productId))
                Discounts[productId] = 0;

            return Discounts[productId];
        }

        public void SetDiscount(int productId, decimal value)
        {
            Discounts[productId] = Math.Clamp(value, 0, 100);
            NotifyStateChanged();
        }

        public HashSet<int> GetSelectedAddOns(int productId)
        {
            if (!SelectedAddOns.ContainsKey(productId))
                SelectedAddOns[productId] = new HashSet<int>();

            return SelectedAddOns[productId];
        }
        public HashSet<int> GetSelectedMonitoring(int productId)
        {
            if (!SelectedMonitoringServices.ContainsKey(productId)) SelectedMonitoringServices[productId] = new HashSet<int>();
            return SelectedMonitoringServices[productId];
        }
        public IEnumerable<int> GetSelectedProductIds()
        {
            return Order.Items
                        .Select(i => i.Product.Id)
                        .Distinct();
        }
        public bool HasAnyProduct()
        {
            return Order.Items.Any();
        }
        public void Reset()
        {
            Order = new Order();
            Products.Clear();
            Quantities.Clear();
            Discounts.Clear();
            SelectedAddOns.Clear();
        }
        public void MarkProductAdded(int productId)
        {
            AddedProducts.Add(productId);
            NotifyStateChanged();
        }
        public void RecalculateMaterials(
       Product p,
       Func<AddOnService, int> calculator)
        {
            foreach (var r in p.RequiredMaterials)
            {
                var minQty = calculator(r);

                RequiredMaterialQty[(p.Id, r.Id)] =
                    Math.Max(
                        GetRequiredQty(p.Id, r.Id, minQty),
                        minQty
                    );
            }

            NotifyStateChanged();
        }
    }
}
