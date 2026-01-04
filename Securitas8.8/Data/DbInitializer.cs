using Microsoft.Data.Sqlite;
using Securitas8._8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Securitas8._8.Data
{
    public static class DbInitializer
    {
        public static void Initialize()
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            PRAGMA foreign_keys = ON;

            /* =========================
               COUNTRIES (MASTER)
            ==========================*/
            CREATE TABLE IF NOT EXISTS Countries (
                Code TEXT PRIMARY KEY,
                Name TEXT NOT NULL
            );

            /* =========================
               ADDRESSES
            ==========================*/
            CREATE TABLE IF NOT EXISTS Addresses (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Street TEXT NOT NULL,
                City TEXT NOT NULL,
                State TEXT NOT NULL,
                PostalCode TEXT NOT NULL,
                Country TEXT NOT NULL
            );

            /* =========================
               CUSTOMERS
            ==========================*/
            CREATE TABLE IF NOT EXISTS Customers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL,
                Phone TEXT NOT NULL,
                AddressId INTEGER NOT NULL,
                FOREIGN KEY (AddressId) REFERENCES Addresses(Id)
            );

            /* =========================
               PRODUCTS
            ==========================*/
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT,
                ImageUrl TEXT,
                Price REAL NOT NULL,
                RequiresSurfaceCalculation INTEGER NOT NULL
            );

            /* =========================
               ADD-ON / SERVICES
            ==========================*/
            CREATE TABLE IF NOT EXISTS AddOnServices (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Price REAL NOT NULL,
                IsMandatory INTEGER NOT NULL,
                MinQuantity INTEGER NOT NULL,
                Unit TEXT
            );

            /* =========================
               PRODUCT ↔ SERVICES
            ==========================*/
            CREATE TABLE IF NOT EXISTS ProductServices (
                ProductId INTEGER NOT NULL,
                AddOnServiceId INTEGER NOT NULL,
                ServiceType TEXT NOT NULL,
                PRIMARY KEY (ProductId, AddOnServiceId, ServiceType),
                FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE,
                FOREIGN KEY (AddOnServiceId) REFERENCES AddOnServices(Id)
            );

            /* =========================
               ORDERS
            ==========================*/
            CREATE TABLE IF NOT EXISTS Orders (
                OrderNo INTEGER PRIMARY KEY,
                CustomerId INTEGER NOT NULL,
                SalesPerson TEXT,
                CreatedDate TEXT NOT NULL,
                Status TEXT NOT NULL,
                AddressId INTEGER NOT NULL,
                TotalAmount numeric,
                FOREIGN KEY (CustomerId) REFERENCES Customers(Id),
                FOREIGN KEY (AddressId) REFERENCES Addresses(Id)
            );

            /* =========================
               ORDER ITEMS
            ==========================*/
             CREATE TABLE IF NOT EXISTS OrderItemServices (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderItemId INTEGER NOT NULL,
                AddOnServiceId INTEGER NOT NULL,
                ServiceType TEXT NOT NULL,
                Quantity INTEGER NOT NULL DEFAULT 1,
                FinalCost Numeric NOT NULL,
                FOREIGN KEY (OrderItemId) REFERENCES OrderItems(Id) ON DELETE CASCADE,
                FOREIGN KEY (AddOnServiceId) REFERENCES AddOnServices(Id),
                UNIQUE (OrderItemId, AddOnServiceId, ServiceType)
                );

            /* =========================
               ORDER ITEM SERVICES
            ==========================*/
            CREATE TABLE IF NOT EXISTS OrderItems (
               Id INTEGER PRIMARY KEY AUTOINCREMENT,
               OrderNo INTEGER NOT NULL,
               ProductId INTEGER NOT NULL,
	           OriginalCost Numeric NOT NULL,
               Quantity INTEGER NOT NULL,
               DiscountPercent REAL NOT NULL,
               FinalCost Numeric NOT NULL,
               FOREIGN KEY (OrderNo) REFERENCES Orders(OrderNo) ON DELETE CASCADE,
               FOREIGN KEY (ProductId) REFERENCES Products(Id)
           );

            CREATE TABLE IF NOT EXISTS Outbox (
                Id TEXT PRIMARY KEY,                 -- GUID
                AggregateType TEXT NOT NULL,          -- Order / Customer
                AggregateId TEXT NOT NULL,            -- OrderNo / CustomerId
                EventType TEXT NOT NULL,              -- CREATED / UPDATED / DELETED
                Payload TEXT NOT NULL,                -- JSON snapshot
                OccurredOnUtc TEXT NOT NULL,
                Processed INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS Inbox (
                Id TEXT PRIMARY KEY,                 -- GUID from server
                AggregateType TEXT NOT NULL,
                AggregateId TEXT NOT NULL,
                EventType TEXT NOT NULL,
                Payload TEXT NOT NULL,
                ReceivedOnUtc TEXT NOT NULL,
                Applied INTEGER NOT NULL DEFAULT 0
            );

            /* =========================
               SEED COUNTRIES
            ==========================*/
            INSERT OR IGNORE INTO Countries (Code, Name) VALUES
            ('IN','India'),
            ('US','United States'),
            ('FR','France'),
            ('DE','Germany'),
            ('GB','United Kingdom'),
            ('CA','Canada'),
            ('AU','Australia'),
            ('SG','Singapore'),
            ('AE','United Arab Emirates'),
            ('JP','Japan');
            ";



            cmd.ExecuteNonQuery();

            // -----------------------------
            // Seed Products
            // -----------------------------
           // SeedProducts(conn);

            // -----------------------------
            // Seed Customers + Orders
            // -----------------------------
            //SeedCustomersAndOrders(conn);
        }

        private static void SeedProducts(SqliteConnection conn)
        {
            var products = GetSeedProducts();
            foreach (var p in products)
            {
                // Product
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO Products(Name, Description, ImageUrl, Price, RequiresSurfaceCalculation)
                    VALUES(@name,@desc,@img,@price,@req)";
                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@desc", p.Description);
                cmd.Parameters.AddWithValue("@img", p.ImageUrl);
                cmd.Parameters.AddWithValue("@price", p.Price);
                cmd.Parameters.AddWithValue("@req", p.RequiresSurfaceCalculation ? 1 : 0);
                cmd.ExecuteNonQuery();

                // AddOns / Required / Monitoring services
                SeedAddOnServices(conn, p.AddOns);
                SeedAddOnServices(conn, p.RequiredMaterials);
                SeedAddOnServices(conn, p.MonitoringServices);
            }
        }

        private static void SeedAddOnServices(SqliteConnection conn, List<AddOnService> services)
        {
            foreach (var s in services)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO AddOnServices(Name, Price, IsMandatory, MinQuantity, Unit)
                    VALUES(@name,@price,@mandatory,@min,@unit)";
                cmd.Parameters.AddWithValue("@name", s.Name);
                cmd.Parameters.AddWithValue("@price", s.Price);
                cmd.Parameters.AddWithValue("@mandatory", s.IsMandatory ? 1 : 0);
                cmd.Parameters.AddWithValue("@min", s.MinQuantity);
                cmd.Parameters.AddWithValue("@unit", s.Unit ?? "");
                cmd.ExecuteNonQuery();
            }
        }

        private static void SeedCustomersAndOrders(SqliteConnection conn)
        {
            var orders = GetSeedOrders();
            foreach (var o in orders)
            {
                // Customer Address
                var addrCmd = conn.CreateCommand();
                addrCmd.CommandText = @"
                    INSERT INTO Addresses(Street, City, State, PostalCode, Country)
                    VALUES(@s,@c,@st,@p,@co)";
                addrCmd.Parameters.AddWithValue("@s", o.Customer.Address.Street);
                addrCmd.Parameters.AddWithValue("@c", o.Customer.Address.City);
                addrCmd.Parameters.AddWithValue("@st", o.Customer.Address.State);
                addrCmd.Parameters.AddWithValue("@p", o.Customer.Address.PostalCode);
                addrCmd.Parameters.AddWithValue("@co", o.Customer.Address.Country);
                addrCmd.ExecuteNonQuery();
                long customerAddrId = GetLastInsertId(conn);

                // Customer
                var custCmd = conn.CreateCommand();
                custCmd.CommandText = @"
                    INSERT INTO Customers(Name, Email, Phone, AddressId)
                    VALUES(@n,@e,@ph,@aid)";
                custCmd.Parameters.AddWithValue("@n", o.Customer.Name);
                custCmd.Parameters.AddWithValue("@e", o.Customer.Email);
                custCmd.Parameters.AddWithValue("@ph", o.Customer.Phone);
                custCmd.Parameters.AddWithValue("@aid", customerAddrId);
                custCmd.ExecuteNonQuery();
                long customerId = GetLastInsertId(conn);

                // Order Address
                var orderAddrCmd = conn.CreateCommand();
                orderAddrCmd.CommandText = @"
                    INSERT INTO Addresses(Street, City, State, PostalCode, Country)
                    VALUES(@s,@c,@st,@p,@co)";
                orderAddrCmd.Parameters.AddWithValue("@s", o.Address.Street);
                orderAddrCmd.Parameters.AddWithValue("@c", o.Address.City);
                orderAddrCmd.Parameters.AddWithValue("@st", o.Address.State);
                orderAddrCmd.Parameters.AddWithValue("@p", o.Address.PostalCode);
                orderAddrCmd.Parameters.AddWithValue("@co", o.Address.Country);
                orderAddrCmd.ExecuteNonQuery();
                long orderAddrId = GetLastInsertId(conn);

                // Order
                var orderCmd = conn.CreateCommand();
                orderCmd.CommandText = @"
                    INSERT INTO Orders(OrderNo, CustomerId, SalesPerson, CreatedDate, Status, AddressId)
                    VALUES(@no,@cust,@sp,@dt,@ss,@addr)";
                orderCmd.Parameters.AddWithValue("@no", o.OrderNo);
                orderCmd.Parameters.AddWithValue("@cust", customerId);
                orderCmd.Parameters.AddWithValue("@sp", o.SalesPerson);
                orderCmd.Parameters.AddWithValue("@dt", o.CreatedDate.ToString("s"));
                orderCmd.Parameters.AddWithValue("@ss", "new");
                orderCmd.Parameters.AddWithValue("@addr", orderAddrId);
                orderCmd.ExecuteNonQuery();

                // Order Items
                foreach (var item in o.Items)
                {
                    var itemCmd = conn.CreateCommand();
                    itemCmd.CommandText = @"
                        INSERT INTO OrderItems(OrderNo, ProductId, Quantity, DiscountPercent)
                        VALUES(@o,@p,@q,@d)";
                    itemCmd.Parameters.AddWithValue("@o", o.OrderNo);
                    itemCmd.Parameters.AddWithValue("@p", item.Product.Id);
                    itemCmd.Parameters.AddWithValue("@q", item.Quantity);
                    itemCmd.Parameters.AddWithValue("@d", item.DiscountPercent);
                    itemCmd.ExecuteNonQuery();
                    long orderItemId = GetLastInsertId(conn);

                    InsertItemServices(conn, orderItemId, item.SelectedAddOns, "Selected");
                    InsertItemServices(conn, orderItemId, item.RequiredMaterials, "Required");
                    InsertItemServices(conn, orderItemId, item.MonitoringServices, "Monitoring");
                }
            }
        }

        private static void InsertItemServices(SqliteConnection conn, long orderItemId, List<AddOnService> services, string type)
        {
            foreach (var s in services)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT OR IGNORE INTO OrderItemServices(OrderItemId, AddOnServiceId, ServiceType, Quantity)
                    VALUES(@oi,@aid,@type,@qty)";
                cmd.Parameters.AddWithValue("@oi", orderItemId);
                cmd.Parameters.AddWithValue("@aid", s.Id);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@qty", 1);
                cmd.ExecuteNonQuery();
            }
        }

        private static long GetLastInsertId(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT last_insert_rowid();";
            return (long)cmd.ExecuteScalar();
        }

        // -----------------------------
        // SEED DATA METHODS
        // -----------------------------
        private static List<Product> GetSeedProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Hikvision 2MP Dome Camera",
                    Description = "2MP Full HD | Dome Type | 1080p | IR Night Vision (20m) | Indoor",
                    Price = 1899,
                    ImageUrl = "images/hikvision_2mp_dome.jpg",
                    AddOns = new List<AddOnService>
                    {
                        new AddOnService { Id = 1, Name = "Installation", Price = 500 },
                        new AddOnService { Id = 2, Name = "Cloud Storage (1 Year)", Price = 800 },
                        new AddOnService { Id = 3, Name = "Extended Warranty", Price = 300 }
                    },
                    RequiredMaterials = new List<AddOnService>
                    {
                        new AddOnService { Id = 10, Name = "Cables", Price = 200, IsMandatory = true },
                        new AddOnService { Id = 11, Name = "Pipes", Price = 100, IsMandatory = true },
                        new AddOnService { Id = 12, Name = "Switch", Price = 150, IsMandatory = true }
                    },
                    MonitoringServices = new List<AddOnService>
                    {
                        new AddOnService { Id = 20, Name = "Monitor", Price = 3000 },
                        new AddOnService { Id = 21, Name = "Joystick", Price = 500 },
                        new AddOnService { Id = 22, Name = "SMS Alerts", Price = 100 }
                    }
                }
                // Add more products here as needed
            };
        }

        private static List<Order> GetSeedOrders()
        {
            return new List<Order>
            {
                new Order
                {
                    OrderNo = 1001,
                    SalesPerson = "Ramesh",
                    CreatedDate = DateTime.Today.AddDays(-1),
                    Status = "Completed",
                    Customer = new Customer
                    {
                        Name = "Anil Kumar",
                        Email = "anil@gmail.com",
                        Phone = "9876543210",
                        Address = new Address
                        {
                            Street = "MG Road",
                            City = "Bangalore",
                            State = "Karnataka",
                            PostalCode = "560001",
                            Country = "India"
                        }
                    },
                    Address = new Address
                    {
                        Street = "MG Road",
                        City = "Bangalore",
                        State = "Karnataka",
                        PostalCode = "560001",
                        Country = "India"
                    },
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            Product = new Product { Id = 1, Name = "Hikvision 2MP Dome Camera" },
                            Quantity = 1,
                            DiscountPercent = 10,
                            SelectedAddOns = new List<AddOnService>
                            {
                                new AddOnService { Id = 1 },
                                new AddOnService { Id = 2 },
                                new AddOnService { Id = 3 }
                            },
                            RequiredMaterials = new List<AddOnService>
                            {
                                new AddOnService { Id = 10 },
                                new AddOnService { Id = 11 },
                                new AddOnService { Id = 12 }
                            },
                            MonitoringServices = new List<AddOnService>
                            {
                                new AddOnService { Id = 20 },
                                new AddOnService { Id = 21 },
                                new AddOnService { Id = 22 }
                            }
                        }
                    }
                }
            };
        }
    }
}

