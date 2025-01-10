using System.Security.Claims;

namespace SV21T1080007.Web
{
    public static class WebUserExtension
    {
        /// <summary>
        /// Đọc thông tin của người dùng từ Principal (Giấy chứng nhận)
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public static WebUserData? GetUserData(this ClaimsPrincipal principal)
        {
            try
            {
                if (!principal.Identity.IsAuthenticated || principal == null || principal.Identity == null)
                {
                    return null;
                }

                var userData = new WebUserData();

                userData.UserId = int.TryParse(principal.FindFirstValue(nameof(userData.UserId)), out int userId) ? userId : 0;
                userData.UserName = principal.FindFirstValue(nameof(userData.UserName)) ?? "";
                userData.DisplayName = principal.FindFirstValue(nameof(userData.DisplayName)) ?? "";
                userData.Photo = principal.FindFirstValue(nameof(userData.Photo)) ?? "";
                userData.Email = principal.FindFirstValue(nameof(userData.Email)) ?? "";
                userData.Roles = new List<string>();
                foreach(var role in principal.FindAll(ClaimTypes.Role))
                {
                    userData.Roles.Add(role.Value);
                }
                return userData;
            }
            catch
            {
                return null;
            }
        }
    }
}
