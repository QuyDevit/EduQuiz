﻿using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using BcryptNet = BCrypt.Net.BCrypt;

namespace EduQuiz.Controllers
{
    public class HomeDashboardController : Controller
    {
        private readonly EduQuizDBContext _context;
        public HomeDashboardController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("")]
        public async Task <IActionResult> Index()
        {
            var getsession = HttpContext.Session.GetString("_USERCURRENT");
            if (getsession != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(getsession);
                string email = userInfo?.Email;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                return View(user);
            }
            return RedirectToAction("Index", "Home");
        }

        [Route("user/profile")]
        public async Task<IActionResult> SettingInfo()
        {
            var getsession = HttpContext.Session.GetString("_USERCURRENT");
            if (getsession != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(getsession);
                string email = userInfo?.Email;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                ViewBag.ListTypeAccount = _context.Roles.ToList();
                ViewBag.ListWorkplace = _context.WorkplaceTypes.ToList();
                if (user != null)
                {
                    return View(user);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [Route("user/settings")]
        public async Task<IActionResult> Privacy()
        {
            var getsession = HttpContext.Session.GetString("_USERCURRENT");
            if (getsession != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(getsession);
                string email = userInfo?.Email;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    return View(user);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [Route("user/change-password")]
        public async Task<IActionResult> ChangePassword()
        {
            var getsession = HttpContext.Session.GetString("_USERCURRENT");
            if (getsession != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(getsession);
                string email = userInfo?.Email;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    return View(user);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [Route("user/billing")]
        public async Task<IActionResult> Billing()
        {
            var getsession = HttpContext.Session.GetString("_USERCURRENT");
            if (getsession != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(getsession);
                string email = userInfo?.Email;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    return View(user);
                }
            }
            return RedirectToAction("Index", "Home");
        }
        #region handle
        public async Task<IActionResult> SavePass(int userid,string oldpass, string newpass)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userid);
            if (user == null)
            {
                return Json(new { result = "FAIL", msg = "Lưu thất bại" });
            }
            if (!BcryptNet.Verify(oldpass, user.Password))
            {
                return Json(new { status = false, msg = "Sai mật khẩu" });
            }
            user.Password = BcryptNet.HashPassword(newpass);
            await _context.SaveChangesAsync();
            return Json(new { result = "PASS", msg = "Đổi mật khẩu thành công" });
        }
        public async Task<IActionResult> SavePrivacy(int userid, string type,bool value)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userid);
            if (user == null) {
                return Json(new { result = "FAIL", msg = "Lưu thất bại" });
            }
            dynamic privacySettings = JsonConvert.DeserializeObject<dynamic>(user.PrivacySettings);
            if (privacySettings != null)
            {
                privacySettings[type] = value;
                user.PrivacySettings = JsonConvert.SerializeObject(privacySettings);
            }
            await _context.SaveChangesAsync();
            return Json(new { result = "PASS", msg = "Lưu thành công" });
        }
        public async Task<IActionResult> SaveTypeAccount(int userid, int role)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userid);
            if (user == null)
            {
                return Json(new { result = "FAIL", msg = "Lưu thất bại" });
            }
            user.RoleId = role;
            await _context.SaveChangesAsync();
            return Json(new { result = "PASS", msg = "Lưu thành công" });
        }
        public async Task<IActionResult> SaveTypeWorkplace(int userid, int workplaceid)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userid);
            if (user == null)
            {
                return Json(new { result = "FAIL", msg = "Lưu thất bại" });
            }
            user.WorkplaceTypeId = workplaceid;
            await _context.SaveChangesAsync();
            return Json(new { result = "PASS", msg = "Lưu thành công" });
        }
        public async Task<IActionResult> GetListInterest()
        {
            try
            {
                var listInterest = await _context.Interests.ToListAsync();
                return Json(new { result = "PASS", msg = "Lấy thành công", data = listInterest });
            }
            catch (Exception ) {
                return Json(new { result = "FAIL", msg = "Lấy thất bại"});
            }
        }
        public async Task<IActionResult> EditInterestUser(int userid,List<InterestUser> listfavorite)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u=>u.Id == userid);
                user.Favorite = JsonConvert.SerializeObject(listfavorite);
                _context.SaveChanges();
                return Json(new { result = "PASS", msg = "Lấy thành công", data = listfavorite });
            }
            catch (Exception)
            {
                return Json(new { result = "FAIL", msg = "Lấy thất bại" });
            }
        }
        public async Task<IActionResult> EditName(int userid, string name)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userid);
            if (user == null)
            {
                return Json(new { result = "FAIL", msg = "Lưu thất bại" });
            }
            var flagname = string.Empty;
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                string[] strname = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                user.FirstName = strname.Length > 1
                    ? string.Join(' ', strname.Take(strname.Length - 1))
                    : strname[0];
                user.LastName = strname.Length > 1 ? strname[^1] : string.Empty;

                flagname = name;
            }
            else
            {
                user.FirstName = "";
                user.LastName = "";
            }
            await _context.SaveChangesAsync();
            return Json(new { result = "PASS", msg = "Lưu thành công" ,data = flagname });
        }

        public async Task<IActionResult> SaveInfo([FromForm] string name, [FromForm] IFormFile image)
        {
            try
            {
                var getsession = HttpContext.Session.GetString("_USERCURRENT");
                if (getsession == null)
                {
                    return Json(new { result = "FAIL", msg = "Vui lòng đăng nhập lại" });
                }

                var userInfo = JsonConvert.DeserializeObject<dynamic>(getsession);
                string email = userInfo?.Email;
                if (string.IsNullOrEmpty(email))
                {
                    return Json(new { result = "FAIL", msg = "Email không hợp lệ" });
                }

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    return Json(new { result = "FAIL", msg = "Không tìm thấy người dùng" });
                }
                if (!string.IsNullOrWhiteSpace(name))
                {
                    name = name.Trim();
                    string[] strname = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    user.FirstName = strname.Length > 1
                        ? string.Join(' ', strname.Take(strname.Length - 1))
                        : strname[0];
                    user.LastName = strname.Length > 1 ? strname[^1] : string.Empty;
                }
                else
                {
                    user.FirstName = "";
                    user.LastName = "";
                }
                if (image != null && image.Length > 0)
                {
                    // Đường dẫn thư mục chứa ảnh
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "src", "img", "profiles");

                    // Tạo thư mục nếu không tồn tại
                    Directory.CreateDirectory(uploadsFolder);

                    // Tạo tên file duy nhất
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Lưu file lên server
                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    // Cập nhật đường dẫn ảnh
                    user.ProfilePicture = $"/src/img/profiles/{uniqueFileName}";
                }
                await _context.SaveChangesAsync();
                var usersession = new
                {
                    Email = user.Email,
                    Username = user.Username,
                    Avatar = user.ProfilePicture

                };
                var userInfoJson = JsonConvert.SerializeObject(usersession);
                HttpContext.Session.SetString("_USERCURRENT", userInfoJson);

                return Json(new { result = "PASS", msg = "Lưu thông tin thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = $"Lưu thất bại: {ex.Message}" });
            }
        }
        #endregion
    }
}
