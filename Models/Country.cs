using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Models
{
    public class Country
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public static List<Country> GetAll() => new()
    {
        new() { Code = "IN", Name = "India" },
        new() { Code = "US", Name = "United States" },
        new() { Code = "FR", Name = "France" },
        new() { Code = "DE", Name = "Germany" },
        new() { Code = "GB", Name = "United Kingdom" },
        new() { Code = "CA", Name = "Canada" },
        new() { Code = "AU", Name = "Australia" },
        new() { Code = "SG", Name = "Singapore" },
        new() { Code = "AE", Name = "United Arab Emirates" },
        new() { Code = "JP", Name = "Japan" }
    };
    }
}
