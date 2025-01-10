using Dapper;
using SV21T1080007.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1080007.DataLayers.SQLServer
{
    public class ProvinceDAL : BaseDAL, ISimpleSelectDAL<Province>
    {
        public ProvinceDAL(string connectionString) : base(connectionString)
        {
        }

        public List<Province> List()
        {
            List<Province> data = new List<Province>();
            using (var connection = OpenConnection())
            {
                var sql = @"select * from Provinces";
                var parameter = new
                {

                };
                data = connection.Query<Province>(sql: sql, param: parameter, commandType: System.Data.CommandType.Text).ToList();
            }
            return data;
        }
    }
}
