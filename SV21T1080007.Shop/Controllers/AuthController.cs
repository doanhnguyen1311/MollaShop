using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SV21T1080007.BusinessLayers;
using SV21T1080007.DomainModels;
using SV21T1080007.Shop.AppCodes;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace SV21T1080007.Shop.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(string username, string password)
        {
            Customer? data = CommonDataService.Login(username, password);
            if (data == null)
            {
                return Json(new { success = false, message = "Tài khoản hoặc mật khẩu không đúng!" });
            }
            else
            {
                ApplicationContext.SetSessionData("user", data);
                return Json(new { success = true, message = "Đăng nhập thành công!" });
            }
        }
        public IActionResult Register(string customerName, string email, string password, string phone, string address, string province, string repeatPassword, string contactName)
        {
            if (password.Equals(repeatPassword))
            {
                var data = new Customer()
                {
                    CustomerName = customerName,
                    Email = email,
                    Phone = phone,
                    Address = address,
                    Province = province,
                    Password = password,
                    ContactName = contactName
                };
                int id = CommonDataService.Register(data);

                if (id > 0)
                {
                    return Json(new { success = true, message = "Đăng ký thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = "Email đã tồn tại!" });
                }
            }
            else
            {
                return Json(new { success = false, message = "Mật khẩu không trùng khớp!" });
            }
        }

        public IActionResult UserInfo()
        {
            var user = ApplicationContext.GetSessionData<Customer>("user");
            if (user == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public IActionResult SaveInfo(Customer data)
        {
            if (data == null)
            {
                return RedirectToAction("Index");
            }
            if (string.IsNullOrWhiteSpace(data.CustomerName))
            {
                ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được rỗng");
            }
            if (string.IsNullOrWhiteSpace(data.ContactName))
            {
                ModelState.AddModelError(nameof(data.ContactName), "Tên giao dịch không được rỗng");
            }
            if (string.IsNullOrWhiteSpace(data.Province))
            {
                ModelState.AddModelError(nameof(data.Province), "Tỉnh thành không được rỗng");
            }

            if (string.IsNullOrWhiteSpace(data.Address))
            {
                ModelState.AddModelError(nameof(data.Address), "Địa chỉ không được rỗng");
            }
            if (string.IsNullOrWhiteSpace(data.Phone))
            {
                ModelState.AddModelError(nameof(data.Phone), "Số điện thoại không được rỗng");
            }
            if (string.IsNullOrWhiteSpace(data.Email))
            {
                ModelState.AddModelError(nameof(data.Email), "Email không được rỗng");
            }
            if (!ModelState.IsValid)
            {
                return View("UserInfo");
            }
            bool result = CommonDataService.UpdateCustomer(data);

            bool k = false;

            Customer cus = CommonDataService.GetCustomer(data.CustomerID);

            if (cus != null && !string.IsNullOrWhiteSpace(cus.Email) && cus.Email.Equals(data.Email))
            {
                k = true;
            }

            if (result == false && !k)
            {
                ModelState.AddModelError(nameof(data.Email), "Email bị trùng");
                return View("UserInfo");
            }
            ApplicationContext.SetSessionData("user", data);
            return View("UserInfo");
        }

        public IActionResult ChangePassword(string current, string newP, string confirm)
        {
            var user = ApplicationContext.GetSessionData<Customer>("user");

            if (user == null)
            {
                return RedirectToAction("Index");
            }
            if (!current.Equals(user.Password))
            {
                return Json(new {success = false, message = "Mật khẩu hiện tại không đúng!" });
            }
            else
            {
                if (!newP.Equals(confirm))
                {
                    return Json(new { success = false, message = "Mật khẩu không trùng khớp!"});
                }
                else
                {
                    if (newP.Equals(user.Password))
                    {
                        return Json(new { success = false, message = "Mật khẩu thay đổi phải khác với mật khẩu hiện tại!" });
                    }
                    else
                    {
                        bool result = UserDataService.ChangePassword(newP, user.CustomerID);

                        if (result)
                        {
                            user.Password = newP;
                            ApplicationContext.SetSessionData("user", user);
                            return Json(new { success = true, message = "Đổi mật khẩu thành công" });
                        }
                    }
                }
            }
                return View();
        }

        public IActionResult Logout()
        {
            ApplicationContext.RemoveSessionData("user");
            return RedirectToAction("Index");
        }
    }
}
