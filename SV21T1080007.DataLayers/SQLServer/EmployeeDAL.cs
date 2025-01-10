using Dapper;
using SV21T1080007.DomainModels;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1080007.DataLayers.SQLServer
{
    public class EmployeeDAL : BaseDAL, ICommonDAL<Employee>
    {
        public EmployeeDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Employee data)
        {
            int id = 0;
            using (var connection = OpenConnection())
            {
                var sql = @"if exists(select * from Employees where Email = @Email)
                                select -1
                            else
                                begin 
                            insert into Employees(FullName, BirthDate, Address, Phone, Email,Photo,IsWorking)
                            values (@FullName, @BirthDate, @Address, @Phone, @Email, @Photo, @IsWorking)
                            select @@IDENTITY
                            end";

                var parameter = new
                {
                    FullName = data.FullName ?? "",
                    BirthDate = data.BirthDate,
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    Photo = data.Photo ?? "",
                    IsWorking = data.IsWorking

                };
                id = connection.ExecuteScalar<int>(sql: sql, param: parameter, commandType: CommandType.Text);
                connection.Close();
            }
            return id;
        }

		public int AddParam(Employee data)
		{
			throw new NotImplementedException();
		}

		public int Count(string searchValue = "")
        {
            int count = 0;
            searchValue = $"%{searchValue}%";

            using (var connection = OpenConnection())
            {
                string sql = "select count(*)\r\n                            " +
                    "from Employees\r\n                            " +
                    "where (FullName like @searchValue) or (Address like @searchValue) or (Email like @searchValue)";

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
                var sql = @"delete from Employees where EmployeeID = @EmployeeID";
                var parameter = new
                {
                    EmployeeID = id
                };
                result = connection.Execute(sql: sql, param: parameter, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public Employee? Get(int id)
        {
            Employee data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select * from Employees where EmployeeID = @EmployeeID";

                var parameter = new
                {
                    EmployeeID = id
                };
                data = connection.QueryFirstOrDefault<Employee>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }

            return data;
        }

        public Employee? Get2Param(string param1, string param2)
        {
            throw new NotImplementedException();
        }

        public bool InUsed(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"
                            if exists(select * from Orders where EmployeeID = @EmployeeID)
                                select 1
                            else
                                select 0
                            ";

                var parameter = new
                {
                    EmployeeID = id
                };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return result;
        }

        public List<Employee> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Employee> list = new List<Employee>();
            searchValue = $"%{searchValue}%";
            using (var connection = OpenConnection())
            {
                string sql = "SELECT *\r\nFROM (\r\n    SELECT *,\r\n          " +
                    " ROW_NUMBER() OVER (ORDER BY FullName) AS RowNumber\r\n    " +
                    "FROM Employees\r\n    " +
                    "WHERE (FullName LIKE @searchValue) or (Address LIKE @searchValue) or (Email LIKE @searchValue)\r\n) " +
                    "AS t\r\nWHERE (@pageSize = 0)\r\n      " +
                    "OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)\r\nORDER BY RowNumber;";

                var parameter = new
                {
                    searchValue = searchValue,
                    page = page,
                    pageSize = pageSize,
                };
                list = connection.Query<Employee>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text).ToList();
                connection.Close();
            }

            return list;
        }

        public List<Employee> ListNotParam()
        {
            throw new NotImplementedException();
        }

        public bool Update(Employee data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from Employees where EmployeeID <> @EmployeeID and Email = @Email)
                            begin
                                update Employees
                                set FullName = @FullName,
	                                Address = @Address,
	                                BirthDate = @BirthDate,
	                                Email = @Email,
	                                Phone = @Phone,
	                                Photo = @Photo,
	                                IsWorking = @IsWorking
                                where EmployeeID = @EmployeeID 
                            end";

                var parameter = new
                {
                    FullName = data.FullName ?? "",
                    Address = data.Address ?? "",
                    BirthDate = data.BirthDate,
                    Email = data.Email ?? "",
                    Phone = data.Phone ?? "",
                    Photo = data.Photo ?? "",
                    IsWorking = data.IsWorking,
                    EmployeeID = data.EmployeeID,
                };
                result = connection.Execute(sql: sql, param:parameter, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }
    }
}
