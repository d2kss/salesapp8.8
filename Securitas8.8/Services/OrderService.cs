using Securitas8._8.Data.Repositories;
using Securitas8._8.Data.Repositories.Interfaces;
using Securitas8._8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Services
{
    public class OrderService
    {
        //    private readonly List<Order> _orders;
        //    private readonly List<Product> _products;

        //    public OrderService()
        //    {
        //        _products = SeedProducts();
        //        _orders = SeedOrders();
        //    }

        //    public List<Order> GetOrders() => _orders;
        //    public List<Product> GetProducts() => _products;

        //    public void AddOrder(Order order)
        //    {
        //        order.OrderNo = _orders.Max(o => o.OrderNo) + 1;
        //        order.CreatedDate = DateTime.Now;
        //        _orders.Add(order);
        //    }

        //    private List<Product> SeedProducts() =>
        //new()
        //{
        //    new Product
        //    {
        //        Id = 1,
        //        Name = "Hikvision 2MP Dome Camera",
        //        Description = "2MP Full HD | Dome Type | 1080p | IR Night Vision (20m) | Indoor",
        //        Price = 1899,
        //        ImageUrl = "images/hikvision_2mp_dome.jpg",
        //        AddOns = new()
        //        {
        //            new AddOnService { Id = 1, Name = "Installation", Price = 500 },
        //            new AddOnService { Id = 2, Name = "Cloud Storage (1 Year)", Price = 800 },
        //            new AddOnService { Id = 3, Name = "Extended Warranty", Price = 300 }
        //        },
        //           RequiredMaterials = new()
        //            {
        //                new AddOnService { Id = 10, Name = "Cables", Price = 200, IsMandatory = true },
        //                new AddOnService { Id = 11, Name = "Pipes", Price = 100, IsMandatory = true },
        //                new AddOnService { Id = 12, Name = "Switch", Price = 150, IsMandatory = true }
        //            },
        //            MonitoringServices = new()
        //            {
        //                new AddOnService { Id = 20, Name = "Monitor", Price = 3000 },
        //                new AddOnService { Id = 21, Name = "Joystick", Price = 500 },
        //                new AddOnService { Id = 22, Name = "SMS Alerts", Price = 100 }
        //            }
        //    },
        //    new Product
        //    {
        //        Id = 2,
        //        Name = "CP Plus 4MP Bullet Camera",
        //        Description = "4MP | Bullet Type | Weatherproof IP67 | Night Vision (30m) | Outdoor",
        //        Price = 2799,
        //        ImageUrl = "images/cpplus_4mp_bullet.jpg",
        //        AddOns = new()
        //        {
        //            new AddOnService { Id = 1, Name = "Installation", Price = 500 },
        //            new AddOnService { Id = 2, Name = "Cloud Storage (1 Year)", Price = 800 },
        //            new AddOnService { Id = 3, Name = "Extended Warranty", Price = 300 }
        //        },
        //           RequiredMaterials = new()
        //            {
        //                new AddOnService { Id = 10, Name = "Cables", Price = 200, IsMandatory = true },
        //                new AddOnService { Id = 11, Name = "Pipes", Price = 100, IsMandatory = true },
        //                new AddOnService { Id = 12, Name = "Switch", Price = 150, IsMandatory = true }
        //            },
        //            MonitoringServices = new()
        //            {
        //                new AddOnService { Id = 20, Name = "Monitor", Price = 3000 },
        //                new AddOnService { Id = 21, Name = "Joystick", Price = 500 },
        //                new AddOnService { Id = 22, Name = "SMS Alerts", Price = 100 }
        //            }
        //    },
        //    new Product
        //    {
        //        Id = 3,
        //        Name = "Dahua 5MP IR Camera",
        //        Description = "5MP | HD Camera | Infrared Night Vision (40m) | Metal Body",
        //        Price = 3499,
        //        ImageUrl = "images/dahua_5mp_ir.jpg",
        //        AddOns = new()
        //        {
        //            new AddOnService { Id = 1, Name = "Installation", Price = 500 },
        //            new AddOnService { Id = 2, Name = "Cloud Storage (1 Year)", Price = 800 },
        //            new AddOnService { Id = 3, Name = "Extended Warranty", Price = 300 }
        //        },
        //           RequiredMaterials = new()
        //            {
        //                new AddOnService { Id = 10, Name = "Cables", Price = 200, IsMandatory = true },
        //                new AddOnService { Id = 11, Name = "Pipes", Price = 100, IsMandatory = true },
        //                new AddOnService { Id = 12, Name = "Switch", Price = 150, IsMandatory = true }
        //            },
        //            MonitoringServices = new()
        //            {
        //                new AddOnService { Id = 20, Name = "Monitor", Price = 3000 },
        //                new AddOnService { Id = 21, Name = "Joystick", Price = 500 },
        //                new AddOnService { Id = 22, Name = "SMS Alerts", Price = 100 }
        //            }
        //    },
        //    new Product
        //    {
        //        Id = 4,
        //        Name = "Hikvision 8MP (4K) Bullet Camera",
        //        Description = "8MP 4K UHD | ColorVu | Outdoor | IP67 | Ultra Low Light",
        //        Price = 6299,
        //        ImageUrl = "images/hikvision_8mp_4k.jpg",
        //        AddOns = new()
        //        {
        //            new AddOnService { Id = 1, Name = "Installation", Price = 500 },
        //            new AddOnService { Id = 2, Name = "Cloud Storage (1 Year)", Price = 800 },
        //            new AddOnService { Id = 3, Name = "Extended Warranty", Price = 300 }
        //        },
        //           RequiredMaterials = new()
        //            {
        //                new AddOnService { Id = 10, Name = "Cables", Price = 200, IsMandatory = true },
        //                new AddOnService { Id = 11, Name = "Pipes", Price = 100, IsMandatory = true },
        //                new AddOnService { Id = 12, Name = "Switch", Price = 150, IsMandatory = true }
        //            },
        //            MonitoringServices = new()
        //            {
        //                new AddOnService { Id = 20, Name = "Monitor", Price = 3000 },
        //                new AddOnService { Id = 21, Name = "Joystick", Price = 500 },
        //                new AddOnService { Id = 22, Name = "SMS Alerts", Price = 100 }
        //            }
        //    },
        //    new Product
        //    {
        //        Id = 5,
        //        Name = "CP Plus PTZ Camera",
        //        Description = "2MP PTZ | Pan-Tilt-Zoom | 25x Optical Zoom | Auto Tracking | Outdoor",
        //        Price = 21999,
        //        ImageUrl = "images/cpplus_ptz.jpg",
        //        AddOns = new()
        //        {
        //            new AddOnService { Id = 1, Name = "Installation", Price = 500 },
        //            new AddOnService { Id = 2, Name = "Cloud Storage (1 Year)", Price = 800 },
        //            new AddOnService { Id = 3, Name = "Extended Warranty", Price = 300 }
        //        },
        //           RequiredMaterials = new()
        //            {
        //                new AddOnService { Id = 10, Name = "Cables", Price = 200, IsMandatory = true },
        //                new AddOnService { Id = 11, Name = "Pipes", Price = 100, IsMandatory = true },
        //                new AddOnService { Id = 12, Name = "Switch", Price = 150, IsMandatory = true }
        //            },
        //            MonitoringServices = new()
        //            {
        //                new AddOnService { Id = 20, Name = "Monitor", Price = 3000 },
        //                new AddOnService { Id = 21, Name = "Joystick", Price = 500 },
        //                new AddOnService { Id = 22, Name = "SMS Alerts", Price = 100 }
        //            }
        //    },
        //    new Product
        //    {
        //        Id = 6,
        //        Name = "IMOU Wi-Fi Smart Camera",
        //        Description = "2MP | Wi-Fi Enabled | Mobile App | Motion Detection | Two-Way Audio",
        //        Price = 2499,
        //        ImageUrl = "images/imou_wifi.jpg",
        //        AddOns = new()
        //        {
        //            new AddOnService { Id = 1, Name = "Installation", Price = 500 },
        //            new AddOnService { Id = 2, Name = "Cloud Storage (1 Year)", Price = 800 },
        //            new AddOnService { Id = 3, Name = "Extended Warranty", Price = 300 }
        //        },
        //           RequiredMaterials = new()
        //            {
        //                new AddOnService { Id = 10, Name = "Cables", Price = 200, IsMandatory = true },
        //                new AddOnService { Id = 11, Name = "Pipes", Price = 100, IsMandatory = true },
        //                new AddOnService { Id = 12, Name = "Switch", Price = 150, IsMandatory = true }
        //            },
        //            MonitoringServices = new()
        //            {
        //                new AddOnService { Id = 20, Name = "Monitor", Price = 3000 },
        //                new AddOnService { Id = 21, Name = "Joystick", Price = 500 },
        //                new AddOnService { Id = 22, Name = "SMS Alerts", Price = 100 }
        //            }
        //    }
        //};

        //    private List<Order> SeedOrders() =>
        //        new()
        //        {
        //        new Order
        //        {
        //            OrderNo = 1001,
        //            SalesPerson = "Ramesh",
        //            CreatedDate = DateTime.Today.AddDays(-1),
        //            Status = "Completed",
        //            Customer = new Customer
        //            {
        //                Name = "Anil Kumar",
        //                Email = "anil@gmail.com",
        //                Phone = "9876543210"
        //            },
        //            Items = new()
        //            {
        //                new OrderItem
        //                {
        //                    Product = SeedProducts()[0],
        //                    Quantity = 1,
        //                    DiscountPercent = 10
        //                }
        //            }
        //        }
        //        };
        private readonly OrderRepository _repo;

        public OrderService(OrderRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Save a new order with customer info, order items, and services
        /// </summary>
        public void PlaceOrder(Order order)
        {
            // Business logic: set created date & status
            order.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(order.Status))
                order.Status = "New";

            _repo.Save(order);
        }

        /// <summary>
        /// Fetch full details of a single order by OrderNo
        /// Includes customer, address, items, services
        /// </summary>
        public Order GetOrderByOrderNo(int orderNo)
        {
            return _repo.GetOrderByOrderNo(orderNo);
        }

        /// <summary>
        /// Fetch all orders with full details
        /// </summary>
        public List<Order> GetAllOrders()
        {
            return _repo.GetAllOrders();
        }
    }
}
