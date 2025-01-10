using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1080007.BusinessLayers;
using SV21T1080007.DomainModels;
using SV21T1080007.Web.AppCodes;
using SV21T1080007.Web.Models;
using System.Globalization;

namespace SV21T1080007.Web.Controllers
{
    [Authorize (Roles = $"{WebUserRoles.ADMINISTATOR}")]
    public class EmployeeController : Controller
    {
        private static int PAGE_SIZE = 3;
        private const string EMPLOYEE_SEARCH_CONDITION = "EmployeeSearchCondition";
        public IActionResult Index()
        {
            PaginationSearchInput? condition = ApplicationContext.GetSessionData<PaginationSearchInput>(EMPLOYEE_SEARCH_CONDITION);
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
            var data = CommonDataService.ListOfEmployee(out rowCount, condition.Page, condition.PageSize, condition.SearchValue ?? "");
            EmployeeSearchResult model = new EmployeeSearchResult()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue ?? "",
                RowCount = rowCount,
                Data = data

            };

            //lưu lại session
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH_CONDITION, condition);
            return View(model);
        }
        public IActionResult Create()
        {

            ViewBag.Title = "Thêm nhân viên";
            var data = new Employee()
            {
                EmployeeID = 0,
                IsWorking = false,
            };

            return View("Edit", data);
        }

        public IActionResult Edit(int id = 0)
        {
            ViewBag.Title = "Chỉnh sửa nhân viên";
            var data = CommonDataService.GetEmployee(id);
            if (data == null)
            {
                return RedirectToAction("Index");
            }
            return View(data);
        }

        [HttpPost]
        public IActionResult Save(Employee data, IFormFile photo, string _birthdate)
        {
            ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật nhân viên";

            if (string.IsNullOrWhiteSpace(data.FullName))
            {
                ModelState.AddModelError(nameof(data.FullName), "*");
            }
            if (string.IsNullOrWhiteSpace(data.Email))
            {
                ModelState.AddModelError("InvalidEmail", "*");
            }
            if (string.IsNullOrWhiteSpace(data.Address))
            {
                data.Address = "";
            }
            if (string.IsNullOrWhiteSpace(data.Phone))
            {
                data.Phone = "";
            }


            // xu ly ngay sinh

            DateTime? d = _birthdate.ToDateTime();
            if (d != null)
            {
                if(d.Value.Year < 1900)
                {
                    ModelState.AddModelError("InvalidDate", "Ngày sinh phải từ  01/01/1900");
                }
                data.BirthDate = d.Value;
            }
            else
            {
                ModelState.AddModelError(nameof(data.BirthDate), "*");
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", data);
            }

            if (photo != null && photo.Length > 0)
            {
                var photoName = Path.GetExtension(photo.FileName);
                var fileName = $"{DateTime.Now.Ticks}_{photoName}";

                var filePath = Path.Combine("wwwroot/images/employees", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    photo.CopyTo(stream);
                }

                data.Photo = $"/images/employees/{fileName}";
            }


            if (data.EmployeeID == 0)
            {
                int id = CommonDataService.AddEmployee(data);
                if (id <= 0)
                {
                    ModelState.AddModelError(nameof(data.Email), "Email bị trùng");
                    return View("Edit", data);
                }
            }
            else
            {
                bool result = CommonDataService.UpdateEmployee(data);
                if (!result)
                {
                    ModelState.AddModelError(nameof(data.Email), "Email bị trùng");
                    return View("Edit", data);
                }
            }
            return RedirectToAction("Index");
        }


        public IActionResult Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                CommonDataService.DeleteEmployee(id);
                return RedirectToAction("Index");
            }
            var data = CommonDataService.GetEmployee(id);
            if (data == null)
            {
                return RedirectToAction("Index");
            }
            return View(data);
        }
    }
}
