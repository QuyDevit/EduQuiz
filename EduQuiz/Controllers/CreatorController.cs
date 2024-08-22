using EduQuiz.DatabaseContext;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace EduQuiz.Controllers
{
    public class CreatorController : Controller
    {
        private readonly EduQuizDBContext _context;
        public CreatorController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("creator/{id:guid}")]
        public async  Task<IActionResult> Index(Guid id)
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (sessionData != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
                string email = userInfo?.Email;

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null)
                {
                    ViewBag.Data = new { quizId = id,userId = user.Id};
                    return View();
                }  
            }
            return RedirectToAction("Index", "Home");
        }
        #region handle
        public async Task<IActionResult> saveImgQuestion([FromForm] IFormFile image, [FromForm] string quizid)
        {
            try
            {
                if (image != null && image.Length > 0)
                {
                    // Đường dẫn thư mục chứa ảnh
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "src", "img", "question");

                    // Tạo thư mục nếu không tồn tại
                    Directory.CreateDirectory(uploadsFolder);

                    // Lấy phần đuôi file (file extension)
                    var fileExtension = Path.GetExtension(image.FileName);
                    var uniqueFileName = quizid + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Kiểm tra và xóa file nếu đã tồn tại
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Lưu file lên server
                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    return Json(new { result = "PASS", url = $"/src/img/question/{uniqueFileName}" });
                }

                return Json(new { result = "FAIL", msg = "File image không hợp lệ" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "FAIL", msg = $"Lưu thất bại: {ex.Message}" });
            }
        }
        public async Task<IActionResult> GetListMusic()
        {
            try
            {
                var list = await _context.Musics.ToListAsync();
                return Json(new { data = list, result = "PASS" });
            }
            catch (Exception ) {
                return Json(new { result = "FAIL" });
            }
        }
        public async Task<IActionResult> GetListTheme()
        {
            try
            {
                var list = await _context.Themes.ToListAsync();
                return Json(new { data = list, result = "PASS" });
            }
            catch (Exception)
            {
                return Json(new { result = "FAIL" });
            }
        }
        #endregion
    }
}
