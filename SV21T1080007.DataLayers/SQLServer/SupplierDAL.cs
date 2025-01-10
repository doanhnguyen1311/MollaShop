using Azure;
using Dapper;
using SV21T1080007.DomainModels;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV21T1080007.DataLayers.SQLServer
{
    public class SupplierDAL : BaseDAL, ICommonDAL<Supplier>
    {
        public SupplierDAL(string connectionString) : base(connectionString)
        {
        }


        public int Add(Supplier data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from Suppliers where Email = @Email)
                                select -1
                            else
                            begin
                                insert into Suppliers(SupplierName, ContactName, Address, Phone, Email,Province)
                                values (@SupplierName, @ContactName, @Address, @Phone, @Email, @Province)
                                select @@IDENTITY
                            end";

                var parameter = new
                {
                    SupplierName = data.SupplierName ?? "",
                    ContactName = data.ContactName ?? "",
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    Province = data.Province ?? ""
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
                string sql = "select count(*)\r\n                                " +
                                "from Suppliers\r\n                                " +
                                "where (SupplierName like @searchValue) ";
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
                var sql = "delete from Suppliers where SupplierID = @SupplierID";
                var parameter = new
                {
                    SupplierID = id
                };

                result = connection.Execute(sql: sql, param: parameter, commandType: System.Data.CommandType.Text) > 0;

                connection.Close();
            }
            return result;
        }

        public Supplier? Get(int id)
        {
            Supplier data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select * from Suppliers where SupplierID = @SupplierID";

                var parameter = new
                {
                    SupplierID = id
                };
                data = connection.QueryFirstOrDefault<Supplier>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return data;
        }

        public bool InUsed(int id)
        {
            throw new NotImplementedException();
        }

        public List<Supplier> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Supplier> list = new List<Supplier>();

            searchValue = $"%{searchValue}%";

            using (var connection = OpenConnection())
            {
                string sql = "SELECT * \r\nFROM (\r\n    " +
                                "SELECT *, \r\n           " +
                                "ROW_NUMBER() OVER (ORDER BY SupplierName) AS RowNumber\r\n    " +
                                "FROM Suppliers\r\n    " +
                                "WHERE (SupplierName LIKE @searchValue) OR (ContactName LIKE @searchValue)\r\n" +
                                ") AS t\r\nWHERE (@pageSize = 0)\r\n   " +
                                "OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)\r\n" +
                                "ORDER BY RowNumber;";

                var parameter = new
                {
                    page = page,
                    searchValue = searchValue,
                    pageSize = pageSize,
                };

                list = connection.Query<Supplier>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text).ToList();
                connection.Close();
            }
            return list;
        }

        public bool Update(Supplier data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from Suppliers where SupplierID <> @SupplierID and Email = @Email)
                            begin
                                update Suppliers
                                set SupplierName = @SupplierName,
	                                Address = @Address,
	                                ContactName = @ContactName,
	                                Email = @Email,
	                                Phone = @Phone,
	                                Province = @Province
                                    where SupplierID = @SupplierID
                            end";

                var parameter = new
                {
                    SupplierName = data.SupplierName ?? "",
                    Address = data.Address ?? "",
                    ContactName = data.ContactName ?? "",
                    Email = data.Email ?? "",
                    Phone = data.Phone ?? "",
                    Province = data.Province ?? "",
                    SupplierID = data.SupplierID,
                };
                result = connection.Execute(sql: sql, param: parameter, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public List<Supplier> ListNotParam()
        {
            List<Supplier> list = new List<Supplier>();

            using (var connection = OpenConnection())
            {
                string sql = "select * from Suppliers";

                list = connection.Query<Supplier>(sql: sql, commandType: System.Data.CommandType.Text).ToList();
                connection.Close();
            }
            return list;
        }

        public Supplier? Get2Param(string param1, string param2)
        {
            throw new NotImplementedException();
        }

		public int AddParam(Supplier data)
		{
			throw new NotImplementedException();
		}
	}
}

