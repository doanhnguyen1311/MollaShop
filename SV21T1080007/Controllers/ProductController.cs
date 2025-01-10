using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1080007.BusinessLayers;
using SV21T1080007.DomainModels;
using SV21T1080007.Web.AppCodes;
using SV21T1080007.Web.Models;
using System.Security.Cryptography.X509Certificates;

namespace SV21T1080007.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.ADMINISTATOR}, {WebUserRoles.EMPLOYEE}")]
    public class ProductController : Controller
    {
        private const int PAGE_SIZE = 10;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";
        public IActionResult Index()
        {
            ProductSearchInput? condition = ApplicationContext.GetSessionData<ProductSearchInput>(PRODUCT_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0,
                };
            }
            return View(condition);
        }

        public IActionResult Search(ProductSearchInput condition)
        {
            int rowCount;
            var data = ProductDataService.ListProducts(
                out rowCount,
                condition.Page,
                condition.PageSize,
                condition.SearchValue ?? "",
                condition.CategoryID,
                condition.SupplierID,
                condition.MinPrice,
                condition.MaxPrice
            );
            ProductSearchResult model = new ProductSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                CategoryID = condition.CategoryID,
                SupplierID = condition.SupplierID,
                MinPrice = condition.MinPrice,
                MaxPrice = condition.MaxPrice,
                Data = data,
                RowCount = rowCount
            };

            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);
            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Thêm sản phẩm mới";
            var data = new Product
            {
                ProductID = 0,
                IsSelling = false,
            };

            return View("Edit", data);
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Chỉnh sửa sản phẩm";
            var data = ProductDataService.GetProduct(id);
            if (data == null)
            {
                return RedirectToAction("Index");
            }
            return View(data);
        }

        [HttpPost]
        public IActionResult Save(Product data, IFormFile photo)
        {
            ViewBag.Title = data.ProductID == 0 ? "Bổ sung sản phẩm" : "Cập nhật sản phẩm";

            if (string.IsNullOrWhiteSpace(data.ProductName))
            {
                ModelState.AddModelError(nameof(data.ProductName), "Tên mặt hàng không được để trống");
            }
            if (string.IsNullOrWhiteSpace(data.Unit))
            {
                ModelState.AddModelError(nameof(data.Unit), "Đơn vị tính không được để trống");
            }
            if (data.CategoryID <= 0)
            {
                ModelState.AddModelError(nameof(data.CategoryID), "Vui lòng chọn loại hàng");
            }
            if (data.SupplierID <= 0)
            {
                ModelState.AddModelError(nameof(data.SupplierID), "Vui lòng chọn nhà cung cấp");
            }
            if (data.Price <= 0)
            {
                ModelState.AddModelError(nameof(data.Price), "Giá tiền phải lớn hơn 0");
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", data);
            }




            if (data.ProductID == 0)
            {
                long id = ProductDataService.AddProduct(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.ProductName), "Tên hàng bị trùng");
                    return View("Edit", data);
                }
                else
                {
                    return RedirectToAction("Edit", new { id = id });
                }
            }
            else
            {
                bool result = ProductDataService.UpdateProduct(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.ProductName), "Tên hàng bị trùng");
                    return View("Edit", data);
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult SavePhoto(ProductPhoto data, IFormFile photo, string method)
        {
            if (data.DisplayOrder <= 0 || string.IsNullOrWhiteSpace(data.DisplayOrder.ToString()))
            {
                ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự không bé hơn 1 hoặc rỗng");
            }

            if ((photo == null || photo.Length == 0) && data.PhotoID == 0)
            {
                ModelState.AddModelError(nameof(data.Photo), "Ảnh không được rỗng");
            }

            if (!ModelState.IsValid)
            {
                return View("Photo", data);
            }

            if (photo != null && photo.Length > 0)
            {
                var photoExtension = Path.GetExtension(photo.FileName);
                var fileName = $"{DateTime.Now.Ticks}{photoExtension}";

                // Đường dẫn lưu ảnh
                var folder1 = Path.Combine("wwwroot/images/products");               // Thư mục Web
                var folder2 = Path.Combine("../SV21T1080007.Shop/wwwroot/images/products");
                System.Diagnostics.Debug.WriteLine($"Đường dẫn đầy đủ folder1: {Path.GetFullPath(folder1)}");// Thư mục Shop
                System.Diagnostics.Debug.WriteLine($"Đường dẫn đầy đủ folder2: {Path.GetFullPath(folder2)}");


                // Lưu file vào cả hai thư mục
                SaveFile(photo, folder1, fileName);
                SaveFile(photo, folder2, fileName);

                data.Photo = $"/images/products/{fileName}";
            }

            if (data.PhotoID == 0)
            {
                ProductDataService.AddPhoto(data);
                return RedirectToAction("Edit", new { id = data.ProductID });
            }
            else
            {
                ProductDataService.UpdatePhoto(data);
                return RedirectToAction("Edit", new { id = data.ProductID });
            }
        }

        // Hàm hỗ trợ lưu file vào thư mục
        private void SaveFile(IFormFile file, string folderPath, string fileName)
        {
            // Kiểm tra và tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Đường dẫn đầy đủ
            var filePath = Path.Combine(folderPath, fileName);

            // Ghi file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
        }


        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                ProductDataService.DeleteProduct(id);
                return RedirectToAction("Index");
            }
            var data = ProductDataService.GetProduct(id);
            if (data == null)
            {
                return RedirectToAction("Index");
            }
            return View(data);
        }


        public IActionResult Photo(int id = 0, string method = "", int photoId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";
                    var data = new ProductPhoto
                    {
                        PhotoID = 0,
                        IsHidden = false,
                        ProductID = id
                    };
                    return View(data);
                case "edit":
                    ViewBag.Title = "Thay đổi ảnh của mặt hàng";
                    var dataUpdate = ProductDataService.GetPhoto(photoId);
                    if (dataUpdate == null)
                    {
                        return RedirectToAction("Edit", dataUpdate);
                    }
                    return View(dataUpdate);
                case "delete":
                    ProductDataService.DeletePhoto(photoId);
                    return RedirectToAction("Edit", new { id });
                default:
                    return RedirectToAction("Index");
            }
        }

        public IActionResult Attribute(int id = 0, string method = "", int attributeId = 0)
        {
            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính cho mặt hàng";
                    var newAttribute = new ProductAttribute
                    {
                        AttributeID = 0,
                        ProductID = id
                    };
                    return View(newAttribute);
                case "edit":
                    ViewBag.Title = "Thay đổi thuộc tính của mặt hàng";
                    var attributeData = ProductDataService.GetAttribute(attributeId);
                    if (attributeData == null)
                    {
                        return RedirectToAction("Edit", new { id });
                    }
                    return View(attributeData);
                case "delete":
                    ProductDataService.DeleteAttribute(attributeId);
                    return RedirectToAction("Edit", new { id });
                default:
                    return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult SaveAttribute(ProductAttribute data)
        {
            ViewBag.Title = data.AttributeID == 0 ? "Bổ sung thuộc tính" : "Cập nhật thuộc tính";

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(data.AttributeName))
            {
                ModelState.AddModelError(nameof(data.AttributeName), "Tên thuộc tính không được để trống");
            }
            if (string.IsNullOrWhiteSpace(data.AttributeValue))
            {
                ModelState.AddModelError(nameof(data.AttributeValue), "Giá trị không được để trống");
            }
            if (data.ProductID <= 0)
            {
                ModelState.AddModelError(nameof(data.ProductID), "Vui lòng chọn sản phẩm");
            }

            if (data.DisplayOrder <= 0 || string.IsNullOrWhiteSpace(data.DisplayOrder.ToString()))
            {
                ModelState.AddModelError(nameof(data.DisplayOrder), "Thứ tự hiển thị phải lớn hơn 0 và không được rỗng");
            }

            if (!ModelState.IsValid)
            {
                return View("Attribute", data);
            }

            if (data.AttributeID == 0)
            {
                long attributeId = ProductDataService.AddAttribute(data);
                if (attributeId <= 0)
                {
                    ModelState.AddModelError(nameof(data.AttributeName), "Tên thuộc tính đã tồn tại");
                    return View("Attribute", data);
                }
            }
            else
            {
                bool result = ProductDataService.UpdateAttribute(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.AttributeName), "Tên thuộc tính đã tồn tại");
                    return View("Attribute", data);
                }
            }

            return RedirectToAction("Edit", new { id = data.ProductID });
        }


    }
}
