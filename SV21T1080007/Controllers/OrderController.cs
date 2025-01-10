using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SV21T1080007.BusinessLayers;
using SV21T1080007.DomainModels;
using SV21T1080007.Web.AppCodes;
using SV21T1080007.Web.Models;
using System.Diagnostics;
using System.Globalization;

namespace SV21T1080007.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.ADMINISTATOR}, {WebUserRoles.EMPLOYEE}")]
    public class OrderController : Controller
    {
        private static int PAGE_SIZE = 20;
        private const int PRODUCT_PAGE_SIZE = 5;
        private const string ORDER_SEARCH_CONDITION = "OrderSearchCondition";
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchSale";
        private const string SHOPPING_CART = "ShoppingCart";

        public IActionResult Index()
        {

            var condition = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH_CONDITION);
            if (condition == null)
            {
                var cultureInfo = new CultureInfo("en-GB");
                var fromDate = DateTime.Today.AddDays(-7).ToString("dd/MM/yyyy", cultureInfo);
                var toDate = DateTime.Today.ToString("dd/MM/yyyy", cultureInfo);
                condition = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    Status = 0,
                    TimeRange = $"{fromDate} - {toDate}",

                };
            }
            return View(condition);
        }

        public IActionResult Search(OrderSearchInput condition)
        {
            if (condition.PageSize <= 0)
            {
                condition.PageSize = PAGE_SIZE; 
            }


            int rowCount;

            var data = OrderDataService.ListOrders(
                out rowCount,
                condition.Page,
                condition.PageSize,
                condition.Status,
                condition.FromTime,
                condition.ToTime,
                condition.SearchValue ?? ""
            );
            var model = new OrderSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                Status = condition.Status,
                TimeRange = condition.TimeRange,
                RowCount = rowCount,
                Data = data
            };

            ApplicationContext.SetSessionData(ORDER_SEARCH_CONDITION, condition);
            return View(model);
        }
        public IActionResult Details(int id = 0)
        {
            ViewBag.IsFinish = false;
            ViewBag.IsDelete = false;
            ViewBag.IsEmployee = false;
            ViewBag.IsEditDetails = false;

            var userData = User.GetUserData();

            var order = OrderDataService.GetOrder(id);
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            if (userData.UserId.Equals(order.EmployeeID.ToString()))
            {
                ViewBag.IsEmployee = true;
            }

            switch (order.Status)
            {
                case Constants.ORDER_INIT:
                    ViewBag.IsDelete = true;
                    ViewBag.IsEditDetails = true;
                    break;
                case Constants.ORDER_ACCEPTED:
                    ViewBag.IsEditDetails = true;
                    break;
                case Constants.ORDER_FINISHED:
                    ViewBag.IsFinish = true;
                    break;
                case Constants.ORDER_CANCEL:
                case Constants.ORDER_REJECTED:
                    ViewBag.IsFinish = true;
                    ViewBag.IsDelete = true;
                    break;

            }

            var details = OrderDataService.ListOrderDetails(id);
            var model = new OrderDetailModel()
            {
                Order = order,
                Details = details
            };

            if (TempData["Message"] != null) ViewBag.Message = TempData["Message"];
            return View(model);
        }

        [HttpGet]
        public IActionResult EditDetail(int id = 0, int productId = 0)
        {
            var model = OrderDataService.GetOrderDetail(id, productId);
            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateDetail(int orderId, int productId, int quantity, decimal salePrice)
        {
            if (quantity <= 0)
            {
                return Json("Số lượng bán không hợp lệ");
            }
            if (salePrice < 0)
                return Json("Giá bán không hợp lệ");
            bool result = OrderDataService.SaveOrderDetail(orderId, productId, quantity, salePrice);
            if (!result)
                return Json("Không được phép thay đổi thông tin của đơn hàng này ");
            return Json("");
        }

        /// <summary>
        /// Xoá mặt hàng ra khỏi đơn hàng 
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <param name="productId">Mã mặt hàng cần tìm</param>
        /// <returns></returns>
        public IActionResult DeleteDetail(int id = 0, int productId = 0)
        {
            bool result = OrderDataService.DeleteOrderDetail(id, productId);
            if (!result)
                TempData["Message"] = "Không thể xoá mặt hàng ra khỏi đơn hàng";
            return RedirectToAction("Details", new { id });
        }
        public IActionResult Create()
        {
            var condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PRODUCT_PAGE_SIZE,
                    SearchValue = ""
                };
            }
            return View(condition);
        }
        public IActionResult SearchProduct(ProductSearchInput condition)
        {
            int rowCount = 0;
            var data = ProductDataService.ListProducts(out rowCount, condition.Page, condition.PageSize, condition.SearchValue ?? "");
            var model = new ProductSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                RowCount = rowCount,
                Data = data
            };
            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);
            return View(model);
        }

        public List<CartItem> GetShoppingCart()
        {
            var shoppingCart = ApplicationContext.GetSessionData<List<CartItem>>(SHOPPING_CART);
            if (shoppingCart == null)
            {
                shoppingCart = new List<CartItem>();
                //shoppingCart.Add(new CartItem { ProductID = 1, ProductName = "abc", Unit = "OK", Quantity = 20});
                ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            }
            return shoppingCart;
        }


        public IActionResult AddToCart(CartItem item)
        {
            if (item.SalePrice < 0 || item.Quantity <= 0)
            {
                return Json("Giá bán và số lượng không hợp lệ");
            }
            var shoppingCart = GetShoppingCart();
            var existsProduct = shoppingCart.FirstOrDefault(m => m.ProductID == item.ProductID);
            if (existsProduct == null)
            {
                shoppingCart.Add(item);
            }
            else
            {
                existsProduct.Quantity += item.Quantity;
                existsProduct.SalePrice = item.SalePrice;
            }
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }


        [HttpGet]
        public IActionResult RemoveFromCart(int id = 0)
        {
            var shoppingCart = GetShoppingCart();
            int index = shoppingCart.FindIndex(m => m.ProductID == id);
            if (index >= 0)
            {
                shoppingCart.RemoveAt(index);
            }
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }

        public IActionResult ClearCart()
        {
            var shoppingCart = GetShoppingCart();
            shoppingCart.Clear();
            ApplicationContext.SetSessionData(SHOPPING_CART, shoppingCart);
            return Json("");
        }

        public IActionResult ShoppingCart()
        {
            var shoppingCart = GetShoppingCart();
            return View(shoppingCart);
        }

        public IActionResult Init(int customerID = 0, string deliveryProvince = "", string deliveryAddress = "")
        {
            var shoppingCart = GetShoppingCart();
            if (shoppingCart.Count == 0)
            {
                return Json("Giỏ hàng trống. Vui lòng chọn mặt hàng cần bán"); ;
            }

            if (customerID == 0 || string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
            {
                return Json("Vui lòng nhập đầy đủ thông tin khách hàng và nơi giao hàng!");
            }

            int employeeID = 1; // TODO: thay bởi ID của nhân viên đang login

            List<OrderDetail> orderDetails = new List<OrderDetail>();
            foreach (var item in shoppingCart)
            {
                orderDetails.Add(new OrderDetail()
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice,
                });
            }
            int orderID = OrderDataService.InitOrder(employeeID, customerID, deliveryProvince, deliveryAddress, orderDetails);
            ClearCart();
            return Json(orderID);
        }

        public IActionResult Accept(int id = 0)
        {

            bool result = OrderDataService.AcceptOrder(id, User.GetUserData().UserId);
            if (!result)

                TempData["Message"] = $"Không thể duyệt đơn hàng này {User.GetUserData().UserId}";
            return RedirectToAction("Details", new { id });

        }
        /// <summary>
        /// Chuyển đơn hàng sang trạng thái đã kết thúc
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Finish(int id = 0)
        {
            bool result = OrderDataService.FinishOrder(id);
            if (!result)

                TempData["Message"] = "Không thể ghi nhận trạng thái đơn hàng kết thúc cho đơn hàng này ";
            return RedirectToAction("Details", new { id });

        }
        /// <summary>
        /// Chuyển đơn hàng sang trạng thái đã bị huỷ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Cancel(int id = 0)
        {
            bool result = OrderDataService.CancelOrder(id);
            if (!result)

                TempData["Message"] = "Không thể thực hiện thao thác huỷ đối với đơn hàng này";
            return RedirectToAction("Details", new { id });

        }
        /// <summary>
        /// Chuyển trang thái đơn hàng sang bị Từ chối 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Reject(int id = 0)
        {
            bool result = OrderDataService.RejectOrder(id);
            if (!result)

                TempData["Message"] = "Không thể thực hiện thao thác từ chối đối với đơn hàng này ";
            return RedirectToAction("Details", new { id });

        }
        /// <summary>
        /// Xoá đơn hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult Delete(int id)
        {
            bool result = OrderDataService.DeleteOrder(id);
            if (!result)
            {
                TempData["Message"] = "Không thể xoá đơn hàng này ";
                return RedirectToAction("Details", new { id });

            }
            return RedirectToAction("Index");

        }

        /// <summary>
        /// Giao diện để chọn người giao hàng cho đơn hàng
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Shipping(int id = 0)
        {
            ViewBag.OrderID = id;
            return View();
        }
        /// <summary>
        /// Ghi nhận giao hàng cho đơn hàng và chuyển đơn giao hàng sang trạng thái đang giao hàng
        /// Hàm trả về chuỗi khác rỗng thông báo lỗi nếu đầu vào không hợp lệ hoặc lỗi ,
        /// Hàm trả về chuỗi rỗng nếu thành công
        /// </summary>
        /// <param name="id"></param>
        /// <param name="shipperId"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Shipping(int id = 0, int shipperId = 0)
        {
            if (shipperId <= 0)
                return Json(new { success = false, message = "Vui lòng chọn người giao hàng" });

            bool result = OrderDataService.ShipOrder(id, shipperId);

            if (!result)
                return Json(new { success = false, message = "Đơn hàng không cho phép chuyển cho người giao hàng" });

            //return Json(new { success = true, redirectUrl = Url.Action("Details", new { id }) });
            return RedirectToAction("Details", new { id });
        }

        /// <summary>
        /// Giao diện để sửa đổi địa chỉ giao hàng
        /// </summary>
        /// <param name="id">Mã đơn hàng</param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult EditAddress(int id = 0)
        {
            var model = OrderDataService.GetOrder(id);
            return View(model);
        }

        /// <summary>
        /// Cập nhật địa chỉ và tỉnh thành giao trong đơn hàng.
        /// Hàm trả về chuỗi khác rỗng thông báo lỗi nếu đầu vào không hợp lệ hoặc lỗi,
        /// hàm trả về chuỗi rỗng nếu thành công
        /// </summary>
        /// <param name="orderID"></param>  
        /// <param name="deliveryAddress"></param>
        /// <param name="deliveryProvince"></param>
        /// <returns></returns>
        public IActionResult UpdateAddress(int orderID, string deliveryAddress, string deliveryProvince)
        {
            if (string.IsNullOrWhiteSpace(deliveryAddress))
                return Json(new { success = false, message = "Vui lòng nhập địa chỉ" });

            if (string.IsNullOrEmpty(deliveryProvince))
                return Json(new { success = false, message = "Vui lòng chọn tỉnh thành" });

            bool result = OrderDataService.SaveOrderAddress(orderID, deliveryAddress, deliveryProvince);

            if (!result)
                return Json(new { success = false, message = "Không được phép thay đổi thông tin của đơn hàng này" });

            return Json(new { success = true, redirectUrl = $"Order/Details/{orderID}" });
        }
    }
}
