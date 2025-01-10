using Microsoft.Data.SqlClient;

namespace SV21T1080007.DataLayers.SQLServer
{
    public abstract class BaseDAL
    {
        protected string _connectionString = "Server=localhost;Database=LiteCommerceDB;User Id=sa;Password=123;TrustServerCertificate=True;";
        /// <summary>
        /// Constructer / ham tao
        /// </summary>
        /// <param name="connectionString"></param>
        public BaseDAL(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// tạo và mở kết nối đến CSDL
        /// </summary>
        /// <returns></returns>
        protected SqlConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = _connectionString;
            connection.Open();
            return connection;

        }
    }
}
