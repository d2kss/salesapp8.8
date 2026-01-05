using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Models
{
    public class AddOnService
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public bool IsMandatory { get; set; } = false;
        public int MinQuantity { get; set; } = 0;
        public string Unit { get; set; } = ""; // meter, pcs, etc.

    }
}
