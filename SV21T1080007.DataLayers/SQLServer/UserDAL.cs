using Dapper;
using SV21T1080007.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1080007.DataLayers.SQLServer
{
    public class UserDAL : BaseDAL, IUserDAL
    {
        public UserDAL(string connectionString) : base(connectionString)
        {
        }

        public int AddOrder(Order data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO Orders (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, Status)
                            VALUES (@CustomerID, GETDATE(), @DeliveryProvince, @DeliveryAddress, @Status);
                            SELECT @@IDENTITY;";
                var parameter = new
                {
                    data.CustomerID,
                    data.DeliveryProvince,
                    data.DeliveryAddress,
                    Status = Constants.ORDER_INIT,
                };

                id = connection.ExecuteScalar<int>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }

        public bool AddOrderDetails(OrderDetail orderDetail)
        {
            bool res = false;
            using (var connection = OpenConnection())
            {
                var sql = @"insert into OrderDetails(OrderID, ProductID, Quantity, SalePrice)
                            values(@OrderID, @ProductID, @Quantity, @SalePrice);";
                var parameter = new
                {
                    OrderID = orderDetail.OrderID,
                    ProductID = orderDetail.ProductID,
                    Quantity = orderDetail.Quantity,
                    SalePrice = orderDetail.SalePrice,
                };

                res = connection.Execute(sql, parameter) > 0;
                connection.Close();
            }
            return res;
        }

        public bool ChangePassword(string pass, int userid)
        {
            bool result = false;

            using (var connection = OpenConnection())
            {
                var sql = @"update Customers
                            set Password = @Password
                            where CustomerID = @CustomerID;";

                var param = new
                {
                    Password = pass,
                    CustomerID = userid,
                };

                result = connection.Execute(sql, param) > 0;
                connection.Close();
            }
            return result;
        }

        public bool DeleteOrder(int id)
        {
            bool result = false;

            using (var connection = OpenConnection())
            {
                var sql = @"Update Orders
                            set Status = @Status
                            WHERE OrderID = @OrderID;";

                var parameter = new
                {
                    Status = Constants.ORDER_CANCEL,
                    OrderID = id
                };

                result = connection.Execute(sql, parameter) > 0;
                connection.Close();
            }
            return result;
        }

        public List<Order> GetAllOrder(int customerID)
        {
            List<Order> orders = new List<Order>();

            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Orders WHERE CustomerID = @CustomerID order by OrderTime desc";

                var parameter = new { CustomerID = customerID };

                orders = connection.Query<Order>(sql, parameter).ToList();
                connection.Close();
            }

            return orders;
        }

        public List<OrderDetail> GetAllOrderDetails(int orderID)
        {
            List<OrderDetail> orderDetails = new List<OrderDetail>();

            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM OrderDetails WHERE OrderID = @OrderID";

                var parameter = new { OrderID = orderID };

                orderDetails = connection.Query<OrderDetail>(sql, parameter).ToList();
                connection.Close();
            }

            return orderDetails;
        }

        public List<Order> GetAllOrdersByParam(int status, DateTime? fromTime, DateTime? toTime, string searchValue, int? customerId)
        {
            List<Order> list = new List<Order>();
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"
                    select o.OrderID, o.OrderTime, o.Status, 
                           c.CustomerName, c.ContactName CustomerContactName, c.Address CustomerAddress, 
                           c.Phone CustomerPhone, c.Email CustomerEmail, 
                           e.FullName EmployeeName, 
                           s.ShipperName, s.Phone ShipperPhone
                    from Orders o
                    left join Customers c on o.CustomerID = c.CustomerID
                    left join Employees e on o.EmployeeID = e.EmployeeID
                    left join Shippers s on o.ShipperID = s.ShipperID
                    where (@Status = 0 or o.Status = @Status)
                      and (@FromTime is null or o.OrderTime >= @FromTime)
                      and (@ToTime is null or o.OrderTime <= @ToTime)
                      and (@SearchValue = N'' 
                           or c.CustomerName like @SearchValue
                           or e.FullName like @SearchValue
                           or s.ShipperName like @SearchValue)
                      and (o.CustomerID = @CustomerID)
                    order by o.OrderTime desc";
                var parameters = new
                {
                    Status = status,
                    FromTime = fromTime,
                    ToTime = toTime,
                    SearchValue = searchValue,
                    CustomerID = customerId
                };

                list = connection.Query<Order>(sql, parameters).ToList();

                connection.Close();
            }

            return list;
        }



        public Order? GetOrder(int customerID)
        {
            Order? order = null;

            using (var connection = OpenConnection())
            {
                var sql = @"SELECT TOP 1 * FROM Orders
                    WHERE CustomerID = @CustomerID
                    ORDER BY OrderTime DESC";  // Lấy đơn hàng gần nhất

                var parameter = new { CustomerID = customerID };

                order = connection.QueryFirstOrDefault<Order>(sql, parameter);
                connection.Close();
            }

            return order;
        }

    }
}
