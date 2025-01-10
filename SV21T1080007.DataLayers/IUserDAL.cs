using Microsoft.VisualBasic.FileIO;
using SV21T1080007.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV21T1080007.DataLayers
{
    public interface IUserDAL
    {
        public bool ChangePassword(string pass, int userid);
        public int AddOrder(Order order);
        public bool AddOrderDetails(OrderDetail orderDetail);
        public List<Order> GetAllOrder(int customerID);
        public List<OrderDetail> GetAllOrderDetails(int orderID);
        public Order? GetOrder(int customerID);
        public bool DeleteOrder(int id);

        public List<Order> GetAllOrdersByParam(int status, DateTime? fromTime, DateTime? toTime , string searchValue, int? customerId);
    }
}
