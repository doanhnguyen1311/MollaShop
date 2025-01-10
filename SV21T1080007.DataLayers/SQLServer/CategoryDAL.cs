using Azure;
using Dapper;
using SV21T1080007.DomainModels;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1080007.DataLayers.SQLServer
{
    public class CategoryDAL : BaseDAL, ICommonDAL<Category>
    {
        public CategoryDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Category data)
        {
            int id = 0;
            using(var connection = OpenConnection())
            {
                var sql = @"if exists(select * from Categories where CategoryName = @CategoryName)
                                select -1
                            else
                            begin
                                insert into Categories(CategoryName, Description) values(@CategoryName, @Description)
                                select @@IDENTITY
                            end";

                var parameter = new
                {
                    CategoryName = data.CategoryName ?? "",
                    Description = data.Description ?? ""
                };

                id = connection.ExecuteScalar<int>(sql:sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }

            return id;
        }

        public int Count(string searchValue = "")
        {
            int count = 0;
            // Add wildcards to the searchValue to make it compatible with the LIKE query
            searchValue = $"%{searchValue}%";

            using (var connection = OpenConnection())
            {
                string sql = @"
                    SELECT COUNT(*)
                    FROM Categories
                    WHERE CategoryName LIKE @searchValue or Description LIKE @searchValue";

                var parameter = new { searchValue };
                count = connection.ExecuteScalar<int>(sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return count;
        }

        public bool Delete(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"delete from Categories where CategoryID = @CategoryID";
                var parameter = new
                {
                    CategoryID = id
                };
                result = connection.Execute(sql: sql, param: parameter, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public bool InUsed(int id)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"
                            if exists(select * from Products where CategoryID = @CategoryID)
                                select 1
                            else
                                select 0
                            ";

                var parameter = new
                {
                    CategoryID = id
                };
                result = connection.ExecuteScalar<bool>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }
            return result;
        }

        public List<Category> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Category> list = new List<Category>();
            searchValue = $"%{searchValue}%";

            using (var connection = OpenConnection())
            {
                string sql = @"
                    SELECT *
                    FROM (
                        SELECT *, ROW_NUMBER() OVER (ORDER BY CategoryName) AS RowNumber
                        FROM Categories
                        WHERE (CategoryName LIKE @searchValue) or (Description LIKE @searchValue)
                    ) AS t
                    WHERE (@pageSize = 0) OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                    ORDER BY RowNumber";

                var parameter = new
                {
                    searchValue,
                    page,
                    pageSize
                };

                list = connection.Query<Category>(sql, param: parameter, commandType: System.Data.CommandType.Text).ToList();
                connection.Close();
            }

            return list;
        }

        public bool Update(Category data)
        {
            bool result = false;
            using (var connection = OpenConnection())
            {
                var sql = @"if not exists(select * from Categories where CategoryID <> @CategoryID and CategoryName = @CategoryName)
                            begin
                                update Categories
                                set CategoryName = @CategoryName,
                                    Description = @Description
                                where CategoryID = @CategoryID
                            end";
                var parameter = new
                {
                    CategoryName = data.CategoryName ?? "",
                    Description = data.Description ?? "",
                    CategoryID = data.CategoryID
                };
                result = connection.Execute(sql: sql, param: parameter, commandType: System.Data.CommandType.Text) > 0;
                connection.Close();
            }
            return result;
        }

        public Category? Get(int id)
        {
            Category data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"select * from Categories where CategoryID = @CategoryID";

                var parameter = new
                {
                    CategoryID = id
                };
                data = connection.QueryFirstOrDefault<Category>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
                connection.Close();
            }

            return data;
        }

        public List<Category> ListNotParam()
        {
            List<Category> list = new List<Category>();

            using (var connection = OpenConnection())
            {
                string sql = @"select * from Categories";

                list = connection.Query<Category>(sql, commandType: System.Data.CommandType.Text).ToList();
                connection.Close();
            }

            return list;
        }

        public Category? Get2Param(string param1, string param2)
        {
            throw new NotImplementedException();
        }

		public int AddParam(Category data)
		{
			throw new NotImplementedException();
		}
	}
}
