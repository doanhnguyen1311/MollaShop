using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1080007.BusinessLayers
{
    public static class Configuration
    {
        private static string connectionString = "Server=localhost;Database=LiteCommerceDB;User Id=sa;Password=123;TrustServerCertificate=True;";

        // khoi tao cau hinh

        public static void Initialize(string connectionString)
        {
            Configuration.connectionString = connectionString;
        }

        // chuoi tham so ket noi

        public static string ConnectionString
        {
            get
            {
                return connectionString;
            }
        }
    }
}
