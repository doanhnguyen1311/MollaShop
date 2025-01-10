using Dapper;
using SV21T1080007.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SV21T1080007.DataLayers.SQLServer
{
    public class OrderDAL : BaseDAL, IOrderDAL
    {
        public OrderDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Order data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"INSERT INTO Orders (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, Status)
                            VALUES (@CustomerID, GETDATE(), @DeliveryProvince, @DeliveryAddress, @EmployeeID, @Status);
                            SELECT @@IDENTITY;";
                var parameter = new
                {
                    data.CustomerID,
                    data.CustomerName,
                    data.CustomerAddress,
                    data.DeliveryProvince,
                    data.DeliveryAddress,
                    data.EmployeeID,
                    Status = Constants.ORDER_INIT,
                };

                id = connection.ExecuteScalar<int>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }

        public int Count(int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            int count = 0;
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"select count(*)
                            from Orders as o
                            left join Customers as c on o.CustomerID = c.CustomerID
                            left join Employees as e on o.EmployeeID = e.EmployeeID
                            left join Shippers as s on o.ShipperID = s.ShipperID

                            where (@Status = 0 or o.Status = @Status)
                            and (@FromTime is null or o.OrderTime >= @FromTime)
                            and (@ToTime is null or o.OrderTime <= @ToTime)
                            and (@SearchValue = N''
                            or c.CustomerName like @SearchValue
                            or e.FullName like @SearchValue
                            or s.ShipperName like @SearchValue)";

                var parameters = new
                {
                    Status = status,
                    FromTime = fromTime,
                    ToTime = toTime,
                    SearchValue = searchValue
                };

                count = connection.ExecuteScalar<int>(sql, parameters);
            }
            return count;
        }

        public bool Delete(int orderID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM OrderDetails WHERE OrderID = @OrderID;
                            DELETE FROM Orders WHERE OrderID = @OrderID;";
                var parameters = new { OrderID = orderID };
                result = connection.Execute(sql, parameters) > 0;
            }
            return result;
        }

        public bool DeleteDetail(int orderID, int productID)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM OrderDetails WHERE OrderID = @OrderID AND ProductID = @ProductID;";
                var parameters = new { OrderID = orderID, ProductID = productID };
                result = connection.Execute(sql, parameters) > 0;
            }
            return result;
        }

        public Order? Get(int orderID)
        {
            Order? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT o.*, c.CustomerName, c.ContactName AS CustomerContactName, c.Address AS CustomerAddress,
                            c.Phone AS CustomerPhone, c.Email AS CustomerEmail, e.FullName AS EmployeeName, s.ShipperName,
                            s.Phone AS ShipperPhone
                            FROM Orders AS o
                            LEFT JOIN Customers AS c ON o.CustomerID = c.CustomerID
                            LEFT JOIN Employees AS e ON o.EmployeeID = e.EmployeeID
                            LEFT JOIN Shippers AS s ON o.ShipperID = s.ShipperID
                            WHERE o.OrderID = @OrderID;";

                var parameters = new { OrderID = orderID };
                data = connection.QuerySingleOrDefault<Order>(sql, parameters);
            }
            return data;
        }

        public OrderDetail? GetDetail(int orderID, int productID)
        {
            OrderDetail? data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT od.*, p.ProductName, p.Photo, p.Unit
                            FROM OrderDetails AS od
                            JOIN Products AS p ON od.ProductID = p.ProductID
                            WHERE od.OrderID = @OrderID AND od.ProductID = @ProductID;";

                var parameters = new { OrderID = orderID, ProductID = productID };
                data = connection.QuerySingleOrDefault<OrderDetail>(sql, parameters);
            }
            return data;
        }

        public IList<Order> List(int page = 1, int pageSize = 0, int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            List<Order> list = new List<Order>();
            if (!string.IsNullOrEmpty(searchValue))
                searchValue = "%" + searchValue + "%";

            using (var connection = OpenConnection())
            {
                var sql = @"select 
                                o.*,
                                c.CustomerName,
                                c.ContactName as CustomerContactName,
                                c.Address as CustomerAddress,
                                c.Phone as CustomerPhone,
                                c.Email as CustomerEmail,
                                e.FullName as EmployeeName,
                                s.ShipperName,
                                s.Phone as ShipperPhone,
                                row_number() over(order by o.OrderTime desc) as RowNumber
                            from Orders as o
                            left join Customers as c on o.CustomerID = c.CustomerID
                            left join Employees as e on o.EmployeeID = e.EmployeeID
                            left join Shippers as s on o.ShipperID = s.ShipperID
                            where 
                                (@Status = 0 or o.Status = @Status)
                                and o.OrderTime >= isnull(@FromTime, o.OrderTime)
                                and o.OrderTime <= isnull(@ToTime, o.OrderTime)
                                and (
                                    @SearchValue = N'' or 
                                    c.CustomerName like @SearchValue or 
                                    e.FullName like @SearchValue or 
                                    s.ShipperName like @SearchValue
                                )
                            order by RowNumber
                            offset (@Page - 1) * @PageSize rows
                            fetch next @PageSize rows only;
                            ";

                var parameters = new
                {
                    Page = page,
                    PageSize = pageSize,
                    Status = status,
                    FromTime = fromTime,
                    ToTime = toTime,
                    SearchValue = searchValue
                };

                list = connection.Query<Order>(sql, parameters, commandType: CommandType.Text).ToList();
                connection.Close();
            }
            return list;
        }

        public IList<OrderDetail> ListDetails(int orderID)
        {
            List<OrderDetail> list = new List<OrderDetail>();
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT od.*, p.ProductName, p.Photo, p.Unit
                            FROM OrderDetails AS od
                            JOIN Products AS p ON od.ProductID = p.ProductID
                            WHERE od.OrderID = @OrderID;";

                var parameters = new { OrderID = orderID };
                list = connection.Query<OrderDetail>(sql, parameters).ToList();
            }
            return list;
        }

        public bool SaveDetail(int orderID, int productID, int quantity, decimal salePrice)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"IF EXISTS (SELECT * FROM OrderDetails WHERE OrderID = @OrderID AND ProductID = @ProductID)
                            UPDATE OrderDetails
                            SET Quantity = @Quantity, SalePrice = @SalePrice
                            WHERE OrderID = @OrderID AND ProductID = @ProductID
                            ELSE
                            INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice)
                            VALUES (@OrderID, @ProductID, @Quantity, @SalePrice);";

                var parameters = new { OrderID = orderID, ProductID = productID, Quantity = quantity, SalePrice = salePrice };
                result = connection.Execute(sql, parameters) > 0;
            }
            return result;
        }

        public bool Update(Order data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"UPDATE Orders
                            SET 
                                CustomerID = @CustomerID,
                                OrderTime = @OrderTime,
                                DeliveryProvince = @DeliveryProvince,
                                DeliveryAddress = @DeliveryAddress,
                                EmployeeID = @EmployeeID,
                                AcceptTime = ISNULL(@AcceptTime, NULL),  -- Chỉ set NULL nếu giá trị AcceptTime là null
                                ShippedTime = ISNULL(@ShippedTime, NULL),  -- Chỉ set NULL nếu giá trị ShippedTime là null
                                FinishedTime = ISNULL(@FinishedTime, NULL),  -- Chỉ set NULL nếu giá trị FinishedTime là null
                                Status = @Status
                            WHERE OrderID = @OrderID;
                            ";

                var parameters = new
                {
                    data.CustomerID,
                    data.OrderTime,
                    data.DeliveryProvince,
                    data.DeliveryAddress,
                    data.EmployeeID,
                    data.AcceptTime,
                    data.ShipperID,
                    data.ShippedTime,
                    data.FinishedTime,
                    data.Status,
                    data.OrderID
                };

                result = connection.Execute(sql, parameters) > 0;
            }
            return result;
        }
    }
}
