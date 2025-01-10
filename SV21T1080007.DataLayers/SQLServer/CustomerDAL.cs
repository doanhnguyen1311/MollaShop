using System;
using System.Net;
using System.Numerics;
using Azure;
using Dapper;
using SV21T1080007.DomainModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV21T1080007.DataLayers.SQLServer
{
    public class CustomerDAL : BaseDAL, ICommonDAL<Customer>
    {
        public CustomerDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Customer data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from Customers where Email = @Email)
                                select -1
                            else
                                begin  
                                    insert into Customers(CustomerName, ContactName, Province, Address, Phone, Email, IsLocked)
                                    values (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked)
                                    select @@IDENTITY
                                end
                            ";
                var parameter = new
                {
                    CustomerName = data.CustomerName ?? "",
                    ContactName = data.ContactName ?? "",
                    Province = data.Province ?? "",
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    IsLocked = data.IsLocked
                };
                id = connection.ExecuteScalar<int>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return id;
        }

		public int AddParam(Customer data)
		{
			int id = 0;
			using (var connection = OpenConnection())
			{
				var sql = @"if exists(select * from Customers where Email = @Email)
                                select -1
                            else
                                begin  
                                    insert into Customers(CustomerName, ContactName, Province, Address, Phone, Email, IsLocked, Password)
                                    values (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked, @Password)
                                    select @@IDENTITY
                                end
                            ";
				var parameter = new
				{
					CustomerName = data.CustomerName ?? "",
					ContactName = data.ContactName ?? "",
					Province = data.Province ?? "",
					Address = data.Address ?? "",
					Phone = data.Phone ?? "",
					Email = data.Email ?? "",
					data.IsLocked,
					Password = data.Password
				};
				id = connection.ExecuteScalar<int>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
				connection.Close();
			}
			return id;
		}

		public int Count(string searchValue = "")
        {
            int count = 0;
            searchValue = $"%{searchValue}%";
            using (var connection = OpenConnection())
            {
                var sql = @"select count(*)
                            from Customers
                            where (CustomerName like @searchValue) or (ContactName like @searchValue)";
                var parameter = new
                {
                    searchValue
                };
                count = connection.ExecuteScalar<int>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);

                connection.Close();

            }
            return count;
        }

        public bool Delete(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"delete from Customers where CustomerID = @CustomerID";
                var parameter = new
                {
                    CustomerID = id
                };
                result = connection.Execute(sql: sql, param: parameter, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
        /// <summary>
        /// Lấy vào một customer dựa vào id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Customer? Get(int id)
        {
            Customer data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select * from Customers where CustomerID = @CustomerID";

                var parameter = new
                {
                    CustomerID = id
                };
                data = connection.QueryFirstOrDefault<Customer>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }

            return data;
        }

        public Customer? Get2Param(string param1, string param2)
        {
            Customer customer = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select * from Customers
                            where Email = @Email and Password=@Password and IsLocked = 0;";
                var parameter = new
                {
                    Email = param1,
                    Password = param2
                };

                customer = connection.QueryFirstOrDefault<Customer>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return customer;
        }

        /// <summary>
        /// kiểm tra xem dữ liệu liên quan của 1 khách hàng nằm ở đâu
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool InUsed(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"
                            if exists(select * from Orders where CustomerID = @CustomerID)
	                            select 1
                            else
	                            select 0
                            ";

                var parameter = new
                {
                    CustomerID = id
                };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return result;
        }

        public List<Customer> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Customer> data = new List<Customer>();
            searchValue = $"%{searchValue}%"; // tìm tương đối với LIKE trong SQL Server

            using (var connection = OpenConnection())
            {
                {
                    var sql = @"select * from(
                                select *,
                                row_number() over(order by CustomerName) as RowNumber
                                from Customers
                                where (CustomerName like @searchValue) or (ContactName like @searchValue)
                                ) as t

                                where (@pageSize = 0)
                                or (RowNumber between (@page - 1) * @pageSize + 1 and @page * @pageSize)
                             order by RowNumber";
                    var parameter = new
                    {
                        page = page, // bên trái: Tên tham số trong câu lệnh SQL, bên phải: giá trị truyền cho tham số
                        pageSize = pageSize,
                        searchValue = searchValue
                    };

                    data = connection.Query<Customer>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text).ToList();

                    connection.Close();
                }

                return data;
            }


        }

        public List<Customer> ListNotParam()
        {
            throw new NotImplementedException();
        }

        public bool Update(Customer data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from Customers where CustomerID <> @CustomerID and Email = @Email)
                            begin
                                update Customers
                                set CustomerName = @CustomerName,
	                                ContactName = @ContactName,
	                                Province = @Province,
	                                Address = @Address,
	                                Phone = @Phone,
	                                Email = @Email,
	                                IsLocked = @IsLocked
                                where CustomerID = @CustomerID
                            end
                            ";
                var parameter = new
                {
                    CustomerName = data.CustomerName ?? "",
                    ContactName = data.ContactName ?? "",
                    Province = data.Province ?? "",
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    IsLocked = data.IsLocked,
                    CustomerID = data.CustomerID
                };
                result = connection.Execute(sql: sql, param: parameter, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
    }
}
