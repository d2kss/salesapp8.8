using Securitas8._8.Data.Repositories.Interfaces;
using Securitas8._8.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Data.Repositories
{
    public class ProductRepository
    {
        public List<Product> GetAll()
        {
            var products = new Dictionary<int, Product>();

            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                    p.Id, p.Name, p.Description, p.ImageUrl, p.Price, p.RequiresSurfaceCalculation,
                    s.Id, s.Name, s.Price, s.IsMandatory, s.MinQuantity, s.Unit,
                    ps.ServiceType
                FROM Products p
                LEFT JOIN ProductServices ps ON ps.ProductId = p.Id
                LEFT JOIN AddOnServices s ON s.Id = ps.AddOnServiceId
                ORDER BY p.Id;
            ";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var productId = reader.GetInt32(0);

                if (!products.TryGetValue(productId, out var product))
                {
                    product = new Product
                    {
                        Id = productId,
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        ImageUrl = reader.GetString(3),
                        Price = reader.GetDecimal(4),
                        RequiresSurfaceCalculation = reader.GetInt32(5) == 1
                    };
                    products.Add(productId, product);
                }

                if (!reader.IsDBNull(6))
                {
                    var service = new AddOnService
                    {
                        Id = reader.GetInt32(6),
                        Name = reader.GetString(7),
                        Price = reader.GetDecimal(8),
                        IsMandatory = reader.GetInt32(9) == 1,
                        MinQuantity = reader.GetInt32(10),
                        Unit = reader.GetString(11)
                    };

                    var type = reader.GetString(12);
                    switch (type)
                    {
                        case "AddOn":
                            product.AddOns.Add(service);
                            break;
                        case "Required":
                            product.RequiredMaterials.Add(service);
                            break;
                        case "Monitoring":
                            product.MonitoringServices.Add(service);
                            break;
                    }
                }
            }

            return products.Values.ToList();
        }
    }
}
