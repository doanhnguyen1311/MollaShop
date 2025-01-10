using SV21T1080007.DataLayers;
using SV21T1080007.DataLayers.SQLServer;
using SV21T1080007.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1080007.BusinessLayers
{
    public static class UserDataService
    {
        private static IUserDAL user;

        static UserDataService()
        {
            string connection = Configuration.ConnectionString;
            user = new UserDAL(connection);
        }

        public static bool ChangePassword(string pass, int userid)
        {
            return user.ChangePassword(pass, userid);
        }
        public static int AddOrder(Order order)
        {
            return user.AddOrder(order);
        }
        public static bool AddOrderDetails(OrderDetail orderDetails)
        {
            return user.AddOrderDetails(orderDetails);
        }
        public static bool DeleteOrder(int id)
        {
            return user.DeleteOrder(id);
        }
        public static List<Order> GetAllOrder(int customerID)
        {
            return user.GetAllOrder(customerID);
        }
        public static List<OrderDetail> GetAllOrderDetails(int orderID)
        {
            return user.GetAllOrderDetails(orderID);
        }

        public static Order? GetOrder(int customerID)
        {
            return user.GetOrder(customerID);
        }

        public static List<Order> GetAllOrdersByParam(int status, DateTime? fromTime, DateTime? toTime, string searchValue, int? customerId)
        {
            return user.GetAllOrdersByParam(status, fromTime, toTime, searchValue, customerId);
        }

    }
}
