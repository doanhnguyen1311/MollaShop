using Microsoft.AspNetCore.Mvc;
using SV21T1080007.BusinessLayers;
using SV21T1080007.DomainModels;
using SV21T1080007.Shop.AppCodes;
using SV21T1080007.Shop.Models;

namespace SV21T1080007.Shop.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            var data = ApplicationContext.GetSessionData<Customer>("user");
            if (data != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Auth");
            }
        }

        public IActionResult Aside()
        {
            var cart = ApplicationContext.GetSessionData<List<CartItem>>("cart") ?? new List<CartItem>();
            return View(cart);
        }

        public IActionResult AddOrder(string deliveryProvince = "", string deliveryAddress = "")
        {
            var user = ApplicationContext.GetSessionData<Customer>("user");
            var cart = ApplicationContext.GetSessionData<List<CartItem>>("cart");

            if (user == null)
            {
                return RedirectToAction("Index", "Auth");
            }
            if (cart == null)
            {
                return RedirectToAction("Index", "Product");
            }


            var order = new Order()
            {
                CustomerID = user.CustomerID,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress,
            };

            int orderID = UserDataService.AddOrder(order);

            foreach (var item in cart)
            {
                var orderDetail = new OrderDetail()
                {
                    OrderID = orderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice,
                };
                bool res = UserDataService.AddOrderDetails(orderDetail);
            }

            ApplicationContext.RemoveSessionData("cart");

            return RedirectToAction("ListOrder");
        }

        public IActionResult ListOrder()
        {
            var user = ApplicationContext.GetSessionData<Customer>("user");
            if (user == null)
            {
                return RedirectToAction("Index", "Auth");
            }

            var order = UserDataService.GetAllOrder(user.CustomerID);

            return View(order);
        }

        public IActionResult OrderDetails(int id=0)
        {
            var user = ApplicationContext.GetSessionData<Customer>("user");
            if(user == null)
            {
                return RedirectToAction("Index", "Auth");
            }

            var orderDetails = UserDataService.GetAllOrderDetails(id);
            return View(orderDetails); 
        }

        public IActionResult DeleteOrder(int id=0)
        {
            var user= ApplicationContext.GetSessionData<Customer>("user");
            if(user == null)
            {
                return RedirectToAction("Index", "Auth");
            }
            bool resID = UserDataService.DeleteOrder(id);

            return RedirectToAction("ListOrder");
        }

        public IActionResult Search(int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            var user = ApplicationContext.GetSessionData<Customer>("user");
            if (user == null)
            {
                return RedirectToAction("Index", "Auth");
            }
            DateTime validFromTime = fromTime ?? new DateTime(1990, 1, 1);
            DateTime validToTime = toTime ?? DateTime.Now;

            List<Order> orders = new List<Order>();
            if (status == 0 && fromTime == null && toTime == null && string.IsNullOrWhiteSpace(searchValue))
            {
                orders = UserDataService.GetAllOrder(user.CustomerID);
            }
            else
            {
                orders = UserDataService.GetAllOrdersByParam(status, validFromTime, validToTime, searchValue, user.CustomerID);
            }

            return View(orders);
        }


    }
}
