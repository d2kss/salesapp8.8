using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string ImageUrl { get; set; } = "";   // local or web image
        public decimal Price { get; set; }
        public List<AddOnService> AddOns { get; set; } = new();
        public List<AddOnService> RequiredMaterials { get; set; } = new();
        public List<AddOnService> MonitoringServices { get; set; } = new();
        public bool RequiresSurfaceCalculation { get; set; }
    }
}
