using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace EduQuiz.Controllers
{
    public class LibraryController : Controller
    {
        private readonly EduQuizDBContext _context;
        public LibraryController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("my-library/eduquizs/all")]
        public async Task<IActionResult> Index()
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (sessionData != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
                string email = userInfo?.Email?.ToString();
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (user != null)
                    {
                        var listEduQuizByUser = await _context.EduQuizs
                            .Where(d => d.UserId == user.Id)
                            .Include(e => e.Questions)
                            .Where(q => q.Type == 1)
                            .ToListAsync();
                        ViewBag.Data = JsonConvert.SerializeObject(new { UserName = user.Username, Avatar = user.ProfilePicture });
                        return View(listEduQuizByUser);
                    }
                }
            }
            return RedirectToAction("Index","Home");
        }
        [Route("my-library/eduquizs/drafts")]
        public async Task<IActionResult> EduQuizDraft()
        {
            var sessionData = HttpContext.Session.GetString("_USERCURRENT");
            if (sessionData != null)
            {
                var userInfo = JsonConvert.DeserializeObject<dynamic>(sessionData);
                string email = userInfo?.Email?.ToString();
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                    if (user != null)
                    {
                        var listEduQuizByUser = await _context.EduQuizs
                            .Where(d => d.UserId == user.Id)
                            .Include(e => e.Questions)
                            .Where(q => q.Type == 0)
                            .ToListAsync();
                        ViewBag.Data = JsonConvert.SerializeObject(new { UserName = user.Username, Avatar = user.ProfilePicture });
                        return View(listEduQuizByUser);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }
        [Route("my-library/eduquizs/{id:int}")]
        public IActionResult LibrarybyFolder(int id)
        {
            return View();
        }
    }
}
