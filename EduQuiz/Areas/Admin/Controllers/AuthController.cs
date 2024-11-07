using EduQuiz.DatabaseContext;
using EduQuiz.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BcryptNet = BCrypt.Net.BCrypt;

namespace EduQuiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly EduQuizDBContext _context; 
        private readonly CookieAuth _cookieAuth;
        public AuthController(EduQuizDBContext context,CookieAuth cookieAuth)
        {
            _cookieAuth = cookieAuth;
            _context = context;
        }
        [Route("admin/auth/fe0f32d2-4d12-4c9d-a7ce-f8b950222e35")]
        public IActionResult Index()
        {
            return View();
        }
        #region handle
        [HttpPost]
        public async Task<IActionResult> CheckLogin(string username, string pass)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(pass))
            {
                return Json(new { status = false, msg = "Vui lòng nhập mật khẩu và tài khoản" });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => (u.Username == username || u.Email == username) && u.RoleId == 5);

            if (user == null)
            {
                return Json(new { status = false, msg = "Sai mật khẩu hoặc tài khoản" });
            }
            if (!BcryptNet.Verify(pass, user.Password))
            {
                return Json(new { status = false, msg = "Sai mật khẩu hoặc tài khoản" });
            }
            var rfToken = _cookieAuth.GenerateRefreshToken();
            user.RefeshToken = rfToken;
            var acToken = _cookieAuth.GenerateToken(user);


            HttpContext.Response.Cookies.Append("acToken", acToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(1)
            });

            HttpContext.Response.Cookies.Append("rfToken", rfToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
            await _context.SaveChangesAsync();
            return Json(new { status = true,redirect = Url.Action("Index", "Home", new {area = "Admin"}) });
        }

        #endregion
    }
}
