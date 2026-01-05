using Microsoft.Data.Sqlite;
using Securitas8._8.Data.Repositories.Interfaces;
using Securitas8._8.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Securitas8._8.Data.Repositories
{
    public class OrderRepository
    {
        public void Save(Order order)
        {

            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                // 1️⃣ ADDRESS
                long addressId = InsertAddress(conn, tx, order.Customer.Address);
                AddAddressOutbox(conn, tx, addressId, order.Customer.Address);

                // 2️⃣ CUSTOMER
                long customerId = InsertCustomer(conn, tx, order.Customer, addressId);
                AddCustomerOutbox(conn, tx, customerId, order.Customer);

                int OrderNo = GetNextOrderNo(conn, tx);
                // 3️⃣ ORDER
                long orderId = InsertOrder(conn, tx, order, OrderNo, customerId, addressId);
                AddOrderOutbox(conn, tx, orderId, order);

                // 4️⃣ ORDER ITEMS + SERVICES
                foreach (var item in order.Items)
                {
                    long orderItemId = InsertOrderItem(conn, tx, orderId, item);
                    AddOrderItemOutbox(conn, tx, orderItemId, item);
                    InsertOrderItemServices(conn, tx, orderItemId, item);
                   
                }

               

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }

            //        using var conn = DatabaseConfig.GetConnection();
            //        conn.Open();
            //        using var tx = conn.BeginTransaction();
            //        long customerId;


            //        // Insert Customer Address
            //        var custAddrCmd = conn.CreateCommand();
            //        custAddrCmd.CommandText = @"
            //        INSERT INTO Addresses
            //        (Street, City, State, PostalCode, Country)
            //        VALUES (@s,@c,@st,@p,@co)";
            //        custAddrCmd.Parameters.AddWithValue("@s", order.Customer.Address.Street);
            //        custAddrCmd.Parameters.AddWithValue("@c", order.Customer.Address.City);
            //        custAddrCmd.Parameters.AddWithValue("@st", order.Customer.Address.State);
            //        custAddrCmd.Parameters.AddWithValue("@p", order.Customer.Address.PostalCode);
            //        custAddrCmd.Parameters.AddWithValue("@co", order.Customer.Address.Country);
            //        custAddrCmd.ExecuteNonQuery();
            //        long customerAddrId = GetLastInsertId(conn);

            //        // Insert Customer
            //        var custCmd = conn.CreateCommand();
            //        custCmd.CommandText = @"
            //        INSERT INTO Customers (Name, Email, Phone, AddressId)
            //        VALUES (@n,@e,@ph,@aid)";
            //        custCmd.Parameters.AddWithValue("@n", order.Customer.Name);
            //        custCmd.Parameters.AddWithValue("@e", order.Customer.Email);
            //        custCmd.Parameters.AddWithValue("@ph", order.Customer.Phone);
            //        custCmd.Parameters.AddWithValue("@aid", customerAddrId);
            //        custCmd.ExecuteNonQuery();

            //        customerId = GetLastInsertId(conn);



            //        // 2️⃣ Save Order
            //        var highOrderNoCmd = conn.CreateCommand();
            //        highOrderNoCmd.CommandText = @"
            //SELECT IFNULL(MAX(OrderNo), 0) 
            //FROM Orders";
            //        var lastOrderNo = Convert.ToInt32(highOrderNoCmd.ExecuteScalar());
            //        var nextOrderNo = lastOrderNo + 1;
            //        var orderCmd = conn.CreateCommand();
            //        orderCmd.CommandText = @"
            //            INSERT INTO Orders
            //            (OrderNo, CustomerId, SalesPerson, CreatedDate, Status,TotalAmount, AddressId)
            //            VALUES (@no,@cust,@sp,@dt,@st,@tmt,@addr)";
            //        orderCmd.Parameters.AddWithValue("@no", nextOrderNo);
            //        orderCmd.Parameters.AddWithValue("@cust", customerId);
            //        orderCmd.Parameters.AddWithValue("@sp", order.SalesPerson);
            //        orderCmd.Parameters.AddWithValue("@dt", order.CreatedDate.ToString("s"));
            //        orderCmd.Parameters.AddWithValue("@st", order.Status);
            //        orderCmd.Parameters.AddWithValue("@tmt", order.TotalAmount);
            //        orderCmd.Parameters.AddWithValue("@addr", customerAddrId);
            //        orderCmd.ExecuteNonQuery();

            //        // 3️⃣ Save Order Items
            //        foreach (var item in order.Items)
            //        {
            //            var itemCmd = conn.CreateCommand();
            //            itemCmd.CommandText = @"
            //                INSERT INTO OrderItems
            //                (OrderNo, ProductId,OriginalCost, Quantity, DiscountPercent,FinalCost)
            //                VALUES (@o,@p,@oc,@q,@d,@fc)";
            //            itemCmd.Parameters.AddWithValue("@o", nextOrderNo);
            //            itemCmd.Parameters.AddWithValue("@p", item.Product.Id);
            //            itemCmd.Parameters.AddWithValue("@oc", item.Product.Price);
            //            itemCmd.Parameters.AddWithValue("@q", item.Quantity);
            //            itemCmd.Parameters.AddWithValue("@d", item.DiscountPercent);
            //            itemCmd.Parameters.AddWithValue("@fc", (item.ProductTotal - item.DiscountAmount));
            //            itemCmd.ExecuteNonQuery();

            //            var orderItemId = GetLastInsertId(conn);

            //            SaveServices(conn, orderItemId, item.SelectedAddOns, "Selected", item);
            //            SaveServices(conn, orderItemId, item.RequiredMaterials, "Required", item);
            //            SaveServices(conn, orderItemId, item.MonitoringServices, "Monitoring", item);
            //        }

            //        tx.Commit();
        }

        private long InsertAddress(SqliteConnection conn, SqliteTransaction tx, Address a)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
        INSERT INTO Addresses (Street, City, State, PostalCode, Country)
        VALUES (@s,@c,@st,@p,@co)";
            cmd.Parameters.AddWithValue("@s", a.Street);
            cmd.Parameters.AddWithValue("@c", a.City);
            cmd.Parameters.AddWithValue("@st", a.State);
            cmd.Parameters.AddWithValue("@p", a.PostalCode);
            cmd.Parameters.AddWithValue("@co", a.Country);
            cmd.ExecuteNonQuery();

            return GetLastInsertId(conn);
            
        }
        private long InsertCustomer(
    SqliteConnection conn,
    SqliteTransaction tx,
    Customer c,
    long addressId)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
        INSERT INTO Customers (Name, Email, Phone, AddressId)
        VALUES (@n,@e,@p,@a)";
            cmd.Parameters.AddWithValue("@n", c.Name);
            cmd.Parameters.AddWithValue("@e", c.Email);
            cmd.Parameters.AddWithValue("@p", c.Phone);
            cmd.Parameters.AddWithValue("@a", addressId);
            cmd.ExecuteNonQuery();

            return GetLastInsertId(conn);
        }

        private int GetNextOrderNo(SqliteConnection conn, SqliteTransaction tx)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "SELECT IFNULL(MAX(OrderNo),0) FROM Orders";
            return Convert.ToInt32(cmd.ExecuteScalar()) + 1;
        }

        private long InsertOrder(
    SqliteConnection conn,
    SqliteTransaction tx,
    Order order,
    int orderNo,
    long customerId,
    long addressId)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
        INSERT INTO Orders
        (OrderNo, CustomerId, SalesPerson, CreatedDate, Status, TotalAmount, AddressId)
        VALUES (@no,@c,@sp,@dt,@st,@t,@a)";
            cmd.Parameters.AddWithValue("@no", orderNo);
            cmd.Parameters.AddWithValue("@c", customerId);
            cmd.Parameters.AddWithValue("@sp", order.SalesPerson);
            cmd.Parameters.AddWithValue("@dt", order.CreatedDate.ToString("s"));
            cmd.Parameters.AddWithValue("@st", order.Status);
            cmd.Parameters.AddWithValue("@t", order.TotalAmount);
            cmd.Parameters.AddWithValue("@a", addressId);
            cmd.ExecuteNonQuery();
            return GetLastInsertId(conn);
        }

        private long InsertOrderItem(
    SqliteConnection conn,
    SqliteTransaction tx,
    long orderNo,
    OrderItem item)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
        INSERT INTO OrderItems
        (OrderNo, ProductId, OriginalCost, Quantity, DiscountPercent, FinalCost)
        VALUES (@o,@p,@oc,@q,@d,@f)";
            cmd.Parameters.AddWithValue("@o", orderNo);
            cmd.Parameters.AddWithValue("@p", item.Product.Id);
            cmd.Parameters.AddWithValue("@oc", item.Product.Price);
            cmd.Parameters.AddWithValue("@q", item.Quantity);
            cmd.Parameters.AddWithValue("@d", item.DiscountPercent);
            cmd.Parameters.AddWithValue("@f", item.ProductFinalCost);
            cmd.ExecuteNonQuery();

            return GetLastInsertId(conn);
        }

        private void InsertOrderItemServices(
    SqliteConnection conn,
    SqliteTransaction tx,
    long orderItemId,
    OrderItem item)
        {
            InsertServices(conn, tx, orderItemId, item.SelectedAddOns, "Selected", item);
            InsertServices(conn, tx, orderItemId, item.RequiredMaterials, "Required", item);
            InsertServices(conn, tx, orderItemId, item.MonitoringServices, "Monitoring", item);
        }

        private void InsertServices(
            SqliteConnection conn,
            SqliteTransaction tx,
            long orderItemId,
            List<AddOnService> services,
            string type,
            OrderItem item)
        {
            foreach (var svc in services)
            {
                int qty = item.GetServiceQty(svc.Id);
                decimal finalCost = svc.Price * qty;

                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
            INSERT INTO OrderItemServices
            (OrderItemId, AddOnServiceId, ServiceType, Quantity, FinalCost)
            VALUES (@oi,@s,@t,@q,@f)";
                cmd.Parameters.AddWithValue("@oi", orderItemId);
                cmd.Parameters.AddWithValue("@s", svc.Id);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.Parameters.AddWithValue("@q", qty);
                cmd.Parameters.AddWithValue("@f", finalCost);
                cmd.ExecuteNonQuery();

                // 🔔 OUTBOX PER SERVICE
                AddOrderItemServiceOutbox(conn, tx, orderItemId, svc, type, qty, finalCost);
            }
        }
        private void AddAddressOutbox(
    SqliteConnection conn,
    SqliteTransaction tx,
    long addressId,
    Address address)
        {
            InsertOutbox(conn, tx,
                aggregateType: "Address",
                aggregateId: addressId.ToString(),
                eventType: "Upsert",
                payload: address);
        }

        private void AddCustomerOutbox(
    SqliteConnection conn,
    SqliteTransaction tx,
    long customerId,
    Customer customer)
        {
            InsertOutbox(conn, tx,
                aggregateType: "Customer",
                aggregateId: customerId.ToString(),
                eventType: "Upsert",
                payload: customer);
        }
        private void AddOrderOutbox(
        SqliteConnection conn,
        SqliteTransaction tx,
        long orderId,
        Order order)
        {
            InsertOutbox(conn, tx,
                aggregateType: "Order",
                aggregateId: orderId.ToString(),
                eventType: "Create",
                payload: order);
        }

        private void AddOrderItemOutbox(
    SqliteConnection conn,
    SqliteTransaction tx,
    long orderItemId,
    OrderItem item)
        {
            InsertOutbox(conn, tx,
                aggregateType: "OrderItem",
                aggregateId: orderItemId.ToString(),
                eventType: "Create",
                payload: item);
        }

        private void AddOrderItemServiceOutbox(
    SqliteConnection conn,
    SqliteTransaction tx,
    long orderItemId,
    AddOnService service,
    string serviceType,
    int quantity,
    decimal cost)
        {
            InsertOutbox(conn, tx,
                aggregateType: "OrderItemService",
                aggregateId: $"{orderItemId}_{service.Id}_{serviceType}",
                eventType: "Create",
                payload: new
                {
                    OrderItemId = orderItemId,
                    ServiceId = service.Id,
                    ServiceType = serviceType,
                    Quantity = quantity,
                    FinalCost = cost
                });
        }

        private void InsertOutbox(
    SqliteConnection conn,
    SqliteTransaction tx,
    string aggregateType,
    string aggregateId,
    string eventType,
    object payload)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
        INSERT INTO Outbox
        (Id, AggregateType, AggregateId, EventType, Payload, OccurredOnUtc)
        VALUES (@id,@type,@aid,@event,@payload,@date);";

            cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString());
            cmd.Parameters.AddWithValue("@type", aggregateType);
            cmd.Parameters.AddWithValue("@aid", aggregateId);
            cmd.Parameters.AddWithValue("@event", eventType);
            cmd.Parameters.AddWithValue("@payload", JsonSerializer.Serialize(payload));
            cmd.Parameters.AddWithValue("@date", DateTime.UtcNow.ToString("o"));

            cmd.ExecuteNonQuery();
        }

        private static long GetLastInsertId(SqliteConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT last_insert_rowid();";
            return (long)cmd.ExecuteScalar();
        }
        private void SaveServices(
            SqliteConnection conn,
            long orderItemId,
            List<AddOnService> services,
            string type,
            OrderItem item)
        {
            foreach (var svc in services)
            {
                var qty = item.GetServiceQty(svc.Id);
                decimal price = 0;
                if (type == "Selected")
                {
                    price = item.AddOnTotal;
                }
                if (type == "Required")
                {
                    price = item.RequiredMaterialTotal;
                }
                if (type == "Monitoring")
                {
                    price = item.MonitoringTotal;
                }
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    INSERT INTO OrderItemServices
                    (OrderItemId, AddOnServiceId, ServiceType, Quantity,FinalCost)
                    VALUES (@oi,@s,@t,@q,@fc)";
                cmd.Parameters.AddWithValue("@oi", orderItemId);
                cmd.Parameters.AddWithValue("@s", svc.Id);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.Parameters.AddWithValue("@q", qty);
                cmd.Parameters.AddWithValue("@fc", price);
                cmd.ExecuteNonQuery();
            }
        }
        public Order GetOrderByOrderNo(int orderNo)
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            var order = new Order();

            // 1️⃣ Get Order + Customer + Address
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT o.OrderNo, o.SalesPerson, o.CreatedDate, o.Status,
               oa.Street, oa.City, oa.State, oa.PostalCode, oa.Country,
               c.Id, c.Name, c.Email, c.Phone,
               ca.Street, ca.City, ca.State, ca.PostalCode, ca.Country
        FROM Orders o
        JOIN Addresses oa ON o.AddressId = oa.Id
        JOIN Customers c ON o.CustomerId = c.Id
        JOIN Addresses ca ON c.AddressId = ca.Id
        WHERE o.OrderNo = @no
    ";
            cmd.Parameters.AddWithValue("@no", orderNo);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null; // Order not found

            // Map order info
            order.OrderNo = reader.GetInt32(0);
            order.SalesPerson = reader.GetString(1);
            order.CreatedDate = DateTime.Parse(reader.GetString(2));
            order.Status = reader.GetString(3);

            // Order Address snapshot
            order.Address = new Address
            {
                Street = reader.GetString(4),
                City = reader.GetString(5),
                State = reader.GetString(6),
                PostalCode = reader.GetString(7),
                Country = reader.GetString(8)
            };

            // Customer info
            order.Customer = new Customer
            {
                Id = reader.GetInt32(9),
                Name = reader.GetString(10),
                Email = reader.GetString(11),
                Phone = reader.GetString(12),
                Address = new Address
                {
                    Street = reader.GetString(13),
                    City = reader.GetString(14),
                    State = reader.GetString(15),
                    PostalCode = reader.GetString(16),
                    Country = reader.GetString(17)
                }
            };

            // 2️⃣ Get OrderItems
            var itemCmd = conn.CreateCommand();
            itemCmd.CommandText = @"
        SELECT oi.Id, oi.ProductId, p.Name, p.Description, p.ImageUrl, p.Price, p.RequiresSurfaceCalculation,
               oi.Quantity, oi.DiscountPercent
        FROM OrderItems oi
        JOIN Products p ON oi.ProductId = p.Id
        WHERE oi.OrderNo = @no
    ";
            itemCmd.Parameters.AddWithValue("@no", orderNo);

            using var itemReader = itemCmd.ExecuteReader();
            while (itemReader.Read())
            {
                var item = new OrderItem
                {
                    Product = new Product
                    {
                        Id = itemReader.GetInt32(1),
                        Name = itemReader.GetString(2),
                        Description = itemReader.GetString(3),
                        ImageUrl = itemReader.GetString(4),
                        Price = itemReader.GetDecimal(5),
                        RequiresSurfaceCalculation = itemReader.GetInt32(6) == 1
                    },
                    Quantity = itemReader.GetInt32(7),
                    DiscountPercent = itemReader.GetDecimal(8)
                };

                // 3️⃣ Get Services for this item
                var svcCmd = conn.CreateCommand();
                svcCmd.CommandText = @"
            SELECT s.Id, s.Name, s.Price, s.IsMandatory, s.MinQuantity, s.Unit, ois.ServiceType, ois.Quantity
            FROM OrderItemServices ois
            JOIN AddOnServices s ON ois.AddOnServiceId = s.Id
            WHERE ois.OrderItemId = @oi
        ";
                svcCmd.Parameters.AddWithValue("@oi", itemReader.GetInt64(0));
                using var svcReader = svcCmd.ExecuteReader();
                while (svcReader.Read())
                {
                    var svc = new AddOnService
                    {
                        Id = svcReader.GetInt32(0),
                        Name = svcReader.GetString(1),
                        Price = svcReader.GetDecimal(2),
                        IsMandatory = svcReader.GetInt32(3) == 1,
                        MinQuantity = svcReader.GetInt32(4),
                        Unit = svcReader.GetString(5)
                    };
                    var type = svcReader.GetString(6);
                    var qty = svcReader.GetInt32(7);
                    item.ServiceQuantities[svc.Id] = qty;

                    switch (type)
                    {
                        case "Selected":
                            item.SelectedAddOns.Add(svc);
                            break;
                        case "Required":
                            item.RequiredMaterials.Add(svc);
                            break;
                        case "Monitoring":
                            item.MonitoringServices.Add(svc);
                            break;
                    }
                }

                order.Items.Add(item);
            }

            return order;
        }

    
        public List<Order> GetAllOrders()
        {
            using var conn = DatabaseConfig.GetConnection();
            conn.Open();

            var orders = new List<Order>();

            // 1️⃣ Load all orders with customer & order address
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT o.OrderNo, o.SalesPerson, o.CreatedDate, o.Status,
               oa.Street, oa.City, oa.State, oa.PostalCode, oa.Country,
               c.Id, c.Name, c.Email, c.Phone,
               ca.Street, ca.City, ca.State, ca.PostalCode, ca.Country
        FROM Orders o
        JOIN Addresses oa ON o.AddressId = oa.Id
        JOIN Customers c ON o.CustomerId = c.Id
        JOIN Addresses ca ON c.AddressId = ca.Id
        ORDER BY o.CreatedDate DESC
    ";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var order = new Order
                {
                    OrderNo = reader.GetInt32(0),
                    SalesPerson = reader.GetString(1),
                    CreatedDate = DateTime.Parse(reader.GetString(2)),
                    Status = reader.GetString(3),
                    Address = new Address
                    {
                        Street = reader.GetString(4),
                        City = reader.GetString(5),
                        State = reader.GetString(6),
                        PostalCode = reader.GetString(7),
                        Country = reader.GetString(8)
                    },
                    Customer = new Customer
                    {
                        Id = reader.GetInt32(9),
                        Name = reader.GetString(10),
                        Email = reader.GetString(11),
                        Phone = reader.GetString(12),
                        Address = new Address
                        {
                            Street = reader.GetString(13),
                            City = reader.GetString(14),
                            State = reader.GetString(15),
                            PostalCode = reader.GetString(16),
                            Country = reader.GetString(17)
                        }
                    },
                    Items = new List<OrderItem>()
                };

                orders.Add(order);
            }

            // 2️⃣ Load all order items and attach to orders
            foreach (var order in orders)
            {
                var itemCmd = conn.CreateCommand();
                itemCmd.CommandText = @"
            SELECT oi.Id, oi.ProductId, p.Name, p.Description, p.ImageUrl, p.Price, p.RequiresSurfaceCalculation,
                   oi.Quantity, oi.DiscountPercent
            FROM OrderItems oi
            JOIN Products p ON oi.ProductId = p.Id
            WHERE oi.OrderNo = @no
        ";
                itemCmd.Parameters.AddWithValue("@no", order.OrderNo);

                using var itemReader = itemCmd.ExecuteReader();
                while (itemReader.Read())
                {
                    var item = new OrderItem
                    {
                        Product = new Product
                        {
                            Id = itemReader.GetInt32(1),
                            Name = itemReader.GetString(2),
                            Description = itemReader.GetString(3),
                            ImageUrl = itemReader.GetString(4),
                            Price = itemReader.GetDecimal(5),
                            RequiresSurfaceCalculation = itemReader.GetInt32(6) == 1
                        },
                        Quantity = itemReader.GetInt32(7),
                        DiscountPercent = itemReader.GetDecimal(8)
                    };

                    long orderItemId = itemReader.GetInt64(0);

                    // 3️⃣ Load services for this item
                    var svcCmd = conn.CreateCommand();
                    svcCmd.CommandText = @"
                SELECT s.Id, s.Name, s.Price, s.IsMandatory, s.MinQuantity, s.Unit, ois.ServiceType, ois.Quantity
                FROM OrderItemServices ois
                JOIN AddOnServices s ON ois.AddOnServiceId = s.Id
                WHERE ois.OrderItemId = @oi
            ";
                    svcCmd.Parameters.AddWithValue("@oi", orderItemId);

                    using var svcReader = svcCmd.ExecuteReader();
                    while (svcReader.Read())
                    {
                        var svc = new AddOnService
                        {
                            Id = svcReader.GetInt32(0),
                            Name = svcReader.GetString(1),
                            Price = svcReader.GetDecimal(2),
                            IsMandatory = svcReader.GetInt32(3) == 1,
                            MinQuantity = svcReader.GetInt32(4),
                            Unit = svcReader.GetString(5)
                        };
                        var type = svcReader.GetString(6);
                        var qty = svcReader.GetInt32(7);
                        item.ServiceQuantities[svc.Id] = qty;

                        switch (type)
                        {
                            case "Selected":
                                item.SelectedAddOns.Add(svc);
                                break;
                            case "Required":
                                item.RequiredMaterials.Add(svc);
                                break;
                            case "Monitoring":
                                item.MonitoringServices.Add(svc);
                                break;
                        }
                    }

                    order.Items.Add(item);
                }
            }

            return orders;
        }


    }

}
