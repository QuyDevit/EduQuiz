using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EduQuiz.Controllers
{
    [CustomAuthorize("User")]
    public class SearchController : Controller
    {
        private readonly EduQuizDBContext _context;
        public SearchController(EduQuizDBContext context) {
            _context = context;
        }
        [Route("search")]
        public async Task<IActionResult> Index()
        {
            var listcategory = await _context.Interests.Where(n=>n.Status == true).ToListAsync();
            return View(listcategory);
        }
        [Route("search-result")]
        public async Task<IActionResult> SearchResult(string query,int? category, string type)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var findAdmin = await _context.Users
                .Where(n => n.Id == 8)
                .Select(n => new { n.ProfilePicture, n.Username })
                .FirstOrDefaultAsync();

            List<CollectionDiscover> listCollection = new();

            if (category.HasValue && category.Value == 0)
            {
                listCollection = await _context.Collections
                    .Select(n => new CollectionDiscover
                    {
                        Topic = n.Topic,
                        Avatar = findAdmin.ProfilePicture,
                        Id = n.Id,
                        ImgCover = n.ImageCover,
                        SumActive = string.IsNullOrEmpty(n.ListEduQuizId)
                            ? 0
                            : JsonConvert.DeserializeObject<List<int>>(n.ListEduQuizId).Count,
                        UserName = findAdmin.Username
                    })
                    .ToListAsync();
            }
            else if (!string.IsNullOrEmpty(query) && string.IsNullOrEmpty(type))
            {
                listCollection = await _context.Collections
                    .Where(n => n.Topic.Contains(query))
                    .Select(n => new CollectionDiscover
                    {
                        Topic = n.Topic,
                        Avatar = findAdmin.ProfilePicture,
                        Id = n.Id,
                        ImgCover = n.ImageCover,
                        SumActive = string.IsNullOrEmpty(n.ListEduQuizId)
                            ? 0
                            : JsonConvert.DeserializeObject<List<int>>(n.ListEduQuizId).Count,
                        UserName = findAdmin.Username
                    })
                    .ToListAsync();
            }

            List<EduQuizItem> listEduQuizbyQuery = new();
            if (string.IsNullOrEmpty(type) || type == "eduquiz" || category.HasValue && category.Value != 0)
            {
                listEduQuizbyQuery = await _context.EduQuizs
                    .Where(n => (n.UserId != 8 && n.Visibility && n.Type == 1) 
                    && (!category.HasValue || n.TopicId == category.Value) 
                    && (string.IsNullOrEmpty(query) || n.Title.Contains(query))) 
                    .Include(n => n.User)
                    .OrderBy(x => Guid.NewGuid())
                    .Select(n => new EduQuizItem
                    {
                        UserName = n.User.Username,
                        Image = n.ImageCover,
                        SumQuestion = _context.Questions.Count(q => q.EduQuizId == n.Id),
                        Title = n.Title,
                        Uuid = n.Uuid,
                    })
                    .ToListAsync();
            }

            List<ProfileDiscover> listProfileUser = new();
            if ((string.IsNullOrEmpty(type) || type == "channel") && !category.HasValue)
            {
                listProfileUser = await _context.Profile
                    .Where(n => n.UserId != 8 &&
                                (string.IsNullOrEmpty(query) ||
                                 n.TitlePage.Contains(query))) 
                    .Include(n => n.User)
                    .Select(n => new ProfileDiscover
                    {
                        Avatar = n.User.ProfilePicture,
                        Id = n.Id,
                        ImgCover = n.Image,
                        TitlePage = n.TitlePage,
                        UserName = n.User.Username,
                        Uuid = n.Uuid,
                        SumEduQuiz = _context.EduQuizs.Count(p => p.UserId == n.UserId),
                    })
                    .ToListAsync();
            }

            var view = new SearchViewModel
            {
                ListCollection = listCollection,
                ListEduQuiz = listEduQuizbyQuery,
                ListProfile = listProfileUser,
            };

            return View(view);
        }
    }
}
