using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV21T1080007.BusinessLayers;
using SV21T1080007.Web.AppCodes;
using System.Diagnostics;

namespace SV21T1080007.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username = "", string password = "")
        {
            ViewBag.UserName = username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập tên và mật khẩu!");
                return View();
            }

            var userAccount = UserAccountService.Authorize(username, password);

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại!");
                return View();
            }

            //Đăng nhập thành công, tạo dữ liệu để lưu thông tin đăng nhập
            var userData = new WebUserData()
            {
                UserId = userAccount.EmployeeID,
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Photo = userAccount.Photo,
                Roles = userAccount.Roles.Split(',').ToList(),
                Email = userAccount.Email,
            };

            ApplicationContext.SetSessionData("employee", userData);
            //Thiết lập phiên đăng nhập cho tài khoản
            await HttpContext.SignInAsync(userData.CreatePrincipal());
            //Redirec về trang chủ sau khi đăng nhập
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            ApplicationContext.RemoveSessionData("employee");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ModelState.AddModelError("Error", "Vui lòng điền đầy đủ thông tin.");
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Mật khẩu không trùng khớp!");
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            // In giá trị Email trong khi debug
            Debug.WriteLine($"Email: {User.GetUserData().Email}, ID: {User.GetUserData().UserId}");

            var result = UserAccountService.ChangePassword(User.GetUserData().Email, oldPassword, newPassword);
            if (result)
            {
                ViewBag.ChangePassSuccess = true;
                return View();
            }
            else
            {
                ModelState.AddModelError("confirmPassword", "Nhập sai mật khẩu cũ");
                if (!ModelState.IsValid)
                {
                    return View();
                }
            }
            return View();
        }



        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
