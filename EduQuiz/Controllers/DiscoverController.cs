using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EduQuiz.Controllers
{
    [CustomAuthorize("User")]
    public class DiscoverController : Controller
    {
        private readonly EduQuizDBContext _context;
        public DiscoverController(EduQuizDBContext context) {
            _context = context;
        }
        [Route("/discover")]
        public async Task<IActionResult> Index()
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index", "Home"); 
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");
            var findAdmin = await _context.Users.Where(n=>n.Id == 8).Select(n=> new {n.ProfilePicture,n.Username}).FirstOrDefaultAsync();
            var listCollection = await _context.Collections.Select(n => new CollectionDiscover
            {
                Topic = n.Topic,
                Avatar=findAdmin.ProfilePicture,
                Id = n.Id,
                ImgCover =n.ImageCover,
                SumActive = string.IsNullOrEmpty(n.ListEduQuizId)
                ? 0
                : JsonConvert.DeserializeObject<List<int>>(n.ListEduQuizId).Count,
                UserName = findAdmin.Username
            }).ToListAsync();
            var listEduQuizRecommend = await _context.EduQuizs
                .Where(n=>n.UserId != 8 && n.Visibility && n.Type == 1)
                .Include(n=>n.User)
                .OrderBy(x => Guid.NewGuid())
                .Select(n => new EduQuizItem { 
                    UserName = n.User.Username,
                    Image = n.ImageCover,
                    SumQuestion = _context.Questions.Count(q => q.EduQuizId == n.Id),
                    Title = n.Title,
                    Uuid = n.Uuid,
                })
                .Take(12)
                .ToListAsync();
            var listEduQuizHot = await _context.EduQuizs
                    .Where(n => n.Visibility && n.Type == 1 && n.UserId != 8)
                    .Include(n => n.User)
                    .Select(n => new EduQuizItem
                    {
                        Uuid = n.Uuid,
                        Title = n.Title,
                        Image = n.ImageCover,
                        UserName = n.User.Username,
                        SumPlay = _context.QuizSessions.Count(p => p.EduQuizId == n.Id),
                        SumQuestion = _context.Questions.Count(q => q.EduQuizId == n.Id)
                    })
                    .OrderByDescending(n => n.SumPlay)
                    .Take(12)
                    .ToListAsync();
            var listProfileUser = await _context.Profile
                .Where(n=>n.UserId != 8)
                .Include(n=>n.User)
                .Select(n=>new ProfileDiscover
                {
                    Avatar = n.User.ProfilePicture,
                    Id = n.Id,  
                    ImgCover = n.Image,
                    TitlePage = n.TitlePage,
                    UserName = n.User.Username,
                    Uuid = n.Uuid,
                    SumEduQuiz = _context.EduQuizs.Count(p => p.UserId == n.UserId),
                }).ToListAsync();
            var view = new DiscoverViewModel
            {
                ListCollection = listCollection,
                ListEduQuizRecommend = listEduQuizRecommend,
                ListEduQuizHot = listEduQuizHot,
                ListProfile = listProfileUser,
            };
            return View(view);

        }
    }
}
