using Dapper;
using SV21T1080007.DomainModels;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SV21T1080007.DataLayers.SQLServer
{
    public class ShipperDAL : BaseDAL, ICommonDAL<Shipper>
    {
        public ShipperDAL(string connectionString) : base(connectionString)
        {
        }

        public int Add(Shipper data)
        {
            using (var connection = OpenConnection())
            {
                var checkPhoneSql = @"IF EXISTS (SELECT * FROM Shippers WHERE Phone = @Phone)
                                      SELECT -1
                                      ELSE
                                      BEGIN
                                          INSERT INTO Shippers(ShipperName, Phone) 
                                          VALUES(@ShipperName, @Phone);
                                          SELECT CAST(SCOPE_IDENTITY() AS int);
                                      END";

                var parameters = new
                {
                    ShipperName = data.ShipperName ?? string.Empty,
                    Phone = data.Phone ?? string.Empty
                };

                // Thực hiện truy vấn và trả về giá trị
                return connection.ExecuteScalar<int>(checkPhoneSql, parameters);
            }
        }

        public bool Update(Shipper data)
        {
            using (var connection = OpenConnection())
            {
                var checkPhoneSql = @"IF EXISTS (SELECT * FROM Shippers WHERE Phone = @Phone AND ShipperID <> @ShipperID)
                                      SELECT -1
                                      ELSE
                                      BEGIN
                                          UPDATE Shippers
                                          SET ShipperName = @ShipperName, Phone = @Phone
                                          WHERE ShipperID = @ShipperID;
                                          SELECT 1; -- Trả về 1 nếu cập nhật thành công
                                      END";

                var parameters = new
                {
                    ShipperName = data.ShipperName ?? string.Empty,
                    Phone = data.Phone ?? string.Empty,
                    ShipperID = data.ShipperID
                };

                // Thực hiện truy vấn và kiểm tra kết quả
                var result = connection.ExecuteScalar<int>(checkPhoneSql, parameters);
                return result > 0; // Trả về true nếu cập nhật thành công
            }
        }

        public int Count(string searchValue = "")
        {
            int count = 0;
            searchValue = $"%{searchValue}%";

            using (var connection = OpenConnection())
            {
                string sql = @"SELECT COUNT(*)
                               FROM Shippers
                               WHERE (ShipperName LIKE @searchValue) OR (Phone LIKE @searchValue)";

                var parameter = new
                {
                    searchValue
                };
                count = connection.ExecuteScalar<int>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
            }
            return count;
        }

        public bool Delete(int id)
        {
            bool result = false;

            using (var connection = OpenConnection())
            {
                var sql = @"DELETE FROM Shippers
                            WHERE ShipperID = @ShipperID";

                var parameter = new
                {
                    ShipperID = id
                };

                result = connection.Execute(sql: sql, param: parameter, commandType: System.Data.CommandType.Text) > 0;
            }

            return result;
        }

        public Shipper? Get(int id)
        {
            Shipper data = null;
            using (var connection = OpenConnection())
            {
                var sql = @"SELECT * FROM Shippers
                            WHERE ShipperID = @ShipperID";

                var parameter = new
                {
                    ShipperID = id
                };
                data = connection.QueryFirstOrDefault<Shipper>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text);
            }
            return data;
        }

        public bool InUsed(int id)
        {
            const string sql = @"IF EXISTS (SELECT * FROM Orders WHERE ShipperID = @ShipperID)
                                 SELECT 1 ELSE SELECT 0";
            var parameters = new { ShipperID = id };

            using (var connection = OpenConnection())
            {
                return connection.ExecuteScalar<bool>(sql, parameters);
            }
        }

        public List<Shipper> List(int page = 1, int pageSize = 0, string searchValue = "")
        {
            List<Shipper> list = new List<Shipper>();
            searchValue = $"%{searchValue}%";
            using (var connection = OpenConnection())
            {
                string sql = @"SELECT *
                               FROM (
                                   SELECT *,
                                   ROW_NUMBER() OVER (ORDER BY ShipperName) AS RowNumber
                                   FROM Shippers
                                   WHERE (ShipperName LIKE @searchValue) OR (Phone LIKE @searchValue)
                               ) AS t
                               WHERE (@pageSize = 0)
                                   OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                               ORDER BY RowNumber;";
                var parameter = new
                {
                    page = page,
                    pageSize = pageSize,
                    searchValue = searchValue
                };

                list = connection.Query<Shipper>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text).ToList();
            }
            return list;
        }

        public List<Shipper> ListNotParam()
        {
            throw new NotImplementedException();
        }

        public Shipper? Get2Param(string param1, string param2)
        {
            throw new NotImplementedException();
        }

		public int AddParam(Shipper data)
		{
			throw new NotImplementedException();
		}
	}
}
