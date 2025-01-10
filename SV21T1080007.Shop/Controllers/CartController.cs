using Microsoft.AspNetCore.Mvc;
using SV21T1080007.BusinessLayers;
using SV21T1080007.Shop.AppCodes;
using SV21T1080007.Shop.Models;

namespace SV21T1080007.Shop.Controllers
{
	public class CartController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult CartItem()
		{
			var cart = ApplicationContext.GetSessionData<List<CartItem>>("cart") ?? new List<CartItem>();
			return View(cart);
		}
        public IActionResult CartHover()
        {
            var cart = ApplicationContext.GetSessionData<List<CartItem>>("cart") ?? new List<CartItem>();

            var distinctProductCount = cart.Select(item => item.ProductID).Distinct().Count();
            ViewBag.TotalQuantity = distinctProductCount;


            return View(cart);
        }

        public IActionResult AddToCart(int productid = 0, int quantity=0)
        {
            int qty = quantity == 0 ? 1 : quantity;

            var cart = ApplicationContext.GetSessionData<List<CartItem>>("cart") ?? new List<CartItem>();

            var cartItem = cart.FirstOrDefault(x => x.ProductID == productid);

            var product = ProductDataService.GetProduct(productid);


            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    ProductID = productid,
                    Quantity = qty,
                    ProductName = product.ProductName,
                    Photo = product.Photo ?? "images/employees/no-photo.png",
                    Unit = product.Unit,
                    SalePrice = product.Price,
                });
            }
            else
            {
                cartItem.Quantity += qty;
            }

            var distinctProductCount = cart.Select(item => item.ProductID).Distinct().Count();
            ViewBag.TotalQuantity = distinctProductCount;


            ApplicationContext.SetSessionData("cart", cart);

            return View("CartHover", cart);
        }

        public IActionResult UpdateQuantity(int productid = 0, int quantity = 0)
		{
			var cart = ApplicationContext.GetSessionData<List<CartItem>>("cart") ?? new List<CartItem>();
			foreach(var item in cart)
			{
				if(item.ProductID == productid)
				{
					item.Quantity = quantity;
				}
			}

			ApplicationContext.SetSessionData("cart", cart);

			return Json(new {success = true});
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

			return View("CartItem", cart);
		}

	}
}
