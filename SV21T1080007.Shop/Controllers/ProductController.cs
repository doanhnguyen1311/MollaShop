using Microsoft.AspNetCore.Mvc;
using SV21T1080007.BusinessLayers;
using SV21T1080007.DomainModels;
using SV21T1080007.Shop.AppCodes;
using SV21T1080007.Shop.Models;
using System.Buffers;

namespace SV21T1080007.Shop.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {

            return View();
        }

        [HttpGet]
        public IActionResult GetListProductByKeyword(string keyword = "")
        {
            var data = ProductDataService.GetListByKeyword(keyword);
            ProductSearchResult model = new ProductSearchResult()
            {
                Data = data
            };
            return View(model);
        }

        public IActionResult GetProductByCategory(int categoryID = 0, int page = 0)
        {
            int rowCount;
            var data = ProductDataService.ListProducts(out rowCount, page, 6, "", categoryID, 0, 0, 999999999999);

            ProductSearchResult model = new ProductSearchResult()
            {
                Page = page,
                PageSize = 6,
                SearchValue = "",
                CategoryID = categoryID,
                SupplierID = 0,
                MinPrice = 0,
                MaxPrice = 999999999999,
                Data = data,
                RowCount = rowCount
            };
            return View(model);
        }

        public IActionResult RemoveCart(int productid)
        {
            var cart = ApplicationContext.GetSessionData<List<CartItem>>("cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(x => x.ProductID == productid);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            ApplicationContext.SetSessionData("cart", cart);

            return Json(new { success = true });
        }

        public IActionResult Details(int id=0)
        {
            var data = ProductDataService.GetProduct(id);
            if (data == null)
            {
                return NotFound("Product not found");
            }

            return View(data);
        }




    }
}
