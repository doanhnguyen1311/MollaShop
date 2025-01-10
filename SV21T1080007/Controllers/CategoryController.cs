using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1080007.BusinessLayers;
using SV21T1080007.DomainModels;
using SV21T1080007.Web.AppCodes;
using SV21T1080007.Web.Models;

namespace SV21T1080007.Web.Controllers
{
    [Authorize(Roles = $"{WebUserRoles.ADMINISTATOR}, {WebUserRoles.EMPLOYEE}")]
    public class CategoryController : Controller
    {
        private const int PAGE_SIZE = 4;
        private const string CATEGORY_SEARCH_CONDITION = "CategorySearchCondition";
        public IActionResult Index()
        {
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<PaginationSearchInput>(CATEGORY_SEARCH_CONDITION);
            if (condition == null)
                condition = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = ""
                };
            return View(condition);

        }

        public IActionResult Search(PaginationSearchInput condition)
        {
            int rowCount;
            var data = CommonDataService.ListOfCategory(out rowCount, condition.Page, condition.PageSize, condition.SearchValue ?? "");
            CategorySearchResult model = new CategorySearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                RowCount = rowCount,
                Data = data

            };

            //lưu lại session
            ApplicationContext.SetSessionData(CATEGORY_SEARCH_CONDITION, condition);
            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Thêm mới loại hàng";
            var data = new Category()
            {
                CategoryID = 0
            };

            return View("Edit", data);
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Chỉnh sửa loại hàng";
            var data = CommonDataService.GetCategory(id);
            if (data == null)
            {
                return RedirectToAction("Index");
            }
            return View(data);
        }

        [HttpPost]
        public IActionResult Save(Category data)
        {
            ViewBag.Title = data.CategoryID == 0 ? "Thêm mới danh mục" : "Cập nhật danh mục";

            // Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(data.CategoryName))
            {
                ModelState.AddModelError(nameof(data.CategoryName), "Tên danh mục không được rỗng");
            }

            if (string.IsNullOrWhiteSpace(data.Description))
            {
                ModelState.AddModelError(nameof(data.Description), "Mô tả không được rỗng");
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", data);
            }

            if (data.CategoryID == 0)
            {
                int id = CommonDataService.AddCategory(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.CategoryName), "Danh mục bị trùng");
                    return View("Edit", data);
                }
            }
            else
            {
                bool res = CommonDataService.UpdateCategory(data);
                if (!res)
                {
                    ModelState.AddModelError(nameof(data.CategoryName), "Danh mục bị trùng");
                    return View("Edit", data);
                }
            }

            return RedirectToAction("Index"); // Chuyển hướng về trang danh sách
        }


        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                CommonDataService.DeleteCategory(id);
                return RedirectToAction("Index");
            }
            var data = CommonDataService.GetCategory(id); 
            if (data == null)
                return RedirectToAction("Index");
            return View(data);
        }
    }
}
