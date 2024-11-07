using EduQuiz.Areas.Admin.Models;
using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EduQuiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CustomAuthorize("Admin")]
    public class CollectionController : Controller
    {
        private readonly EduQuizDBContext _context;
        public CollectionController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("admin/manage/collection")]
        public async Task<IActionResult> Index()
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index","Home");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;

            var listcollection = await _context.Collections.Where(n=>n.ProfileId == 7).Select(n=>new CollectionData
            {
                Id = n.Id,
                Topic = n.Topic,
                ImageCover = n.ImageCover,
                SumActivity = string.IsNullOrEmpty(n.ListEduQuizId)
                ? 0
                : JsonConvert.DeserializeObject<List<int>>(n.ListEduQuizId).Count,
                Status = n.Status,
                CreatedAt = n.CreatedAt,
            }).ToListAsync();
            var listmusic = await _context.Musics.ToListAsync();
            var listtheme = await _context.Themes.ToListAsync();
            var view = new CollectionViewModel { Musics = listmusic, Themes = listtheme,Collections = listcollection };
            return View(view);
        }
        [Route("admin/manage/collection/edit/{id?}")]
        public async Task<IActionResult> AddCollection(int? id)
        {
            CollectionEdit collection = new CollectionEdit();
            if (id.HasValue)
            {
                collection.Collection = await _context.Collections.FindAsync(id.Value);
                collection.ListQuiz = new List<EduQuizProfile>();
                if (collection == null)
                {
                    return NotFound();
                }
                List<int> quizid = JsonConvert.DeserializeObject<List<int>>(collection.Collection.ListEduQuizId);
                foreach (var item in quizid)
                {
                    var eduQuizProfile = await _context.EduQuizs
                    .Where(n => n.Id == item)
                    .Select(n => new EduQuizProfile
                    {
                        Id = n.Id,
                        Image = n.ImageCover,
                        Name = n.Title,
                    })
                    .FirstOrDefaultAsync();
                    if (eduQuizProfile != null)
                    {
                        collection.ListQuiz.Add(eduQuizProfile);
                    }
                }
            }
            else
            {
                collection = null;
            }
            return View(collection);
        }
        #region handle
        public async Task<IActionResult> AddData(string name ,string urlfile,string type,int flag)
        {
            if(flag == 1)
            {
                var checkdata = await _context.Themes.FirstOrDefaultAsync(n=>n.Name == name);
                if (checkdata != null) {
                    return Json(new { status = false });
                }
                var newTheme = new Theme()
                {
                    Name = name,
                    CreatedAt = DateTime.Now,
                    Source = urlfile,
                    Type = type,
                    UpdateAt = DateTime.Now,
                };
                _context.Themes.Add(newTheme);
            }
            else
            {
                var checkdata = await _context.Musics.FirstOrDefaultAsync(n => n.Name == name);
                if (checkdata != null)
                {
                    return Json(new { status = false });
                }
                var newMusic = new Music()
                {
                    Name = name,
                    CreatedAt = DateTime.Now,
                    Source = urlfile,
                    Type = type,
                    UpdateAt = DateTime.Now,
                };
                _context.Musics.Add(newMusic);
            }
            await _context.SaveChangesAsync();
            return Json(new {status = true});
        }
        public async Task<IActionResult> SaveData(int id,string name, string urlfile, string type, int flag)
        {
            if (flag == 1)
            {
                var checkdata = await _context.Themes.FindAsync(id);
                if (checkdata == null)
                {
                    return Json(new { status = false });
                }
                checkdata.Name = name;
                checkdata.Source = urlfile;
                checkdata.Type = type;
            }
            else
            {
                var checkdata = await _context.Musics.FindAsync(id);
                if (checkdata == null)
                {
                    return Json(new { status = false });
                }
                checkdata.Name = name;
                checkdata.Source = urlfile;
                checkdata.Type = type;
            }
            await _context.SaveChangesAsync();
            return Json(new { status = true });
        }
        public async Task<IActionResult> GetEduQuiz()
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { status = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var avatart = jwtToken.Claims.FirstOrDefault(c => c.Type == "Avatar")?.Value;
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
            var iduser = int.Parse(userId ?? "1");
            var listEduquiz = await _context.EduQuizs
             .Where(e => e.UserId == iduser && e.Type == 1)
             .Include(e => e.Questions)
             .ToListAsync();

            var dataresponse = new List<object>();
            foreach (var item in listEduquiz)
            {
                dataresponse.Add(new
                {
                    Id = item.Id,
                    Title = item.Title,
                    Description = item.Description,
                    Image = item.ImageCover,
                    Avatart = avatart,
                    Username = username,
                    CountQuestion = item.Questions.Count,
                });
            }
            return Json(new { status = true, data = dataresponse });
        }
        public async Task<IActionResult> AddDataCollection(string name, string img,List<int> listquizid)
        {
            if(string.IsNullOrEmpty(name) || string.IsNullOrEmpty(img) || listquizid.Count < 1)
            {
                return Json(new { status = false });
            }
            var newCollection = new Collection()
            {
                Topic =name,
                ImageCover =img,
                ListEduQuizId = JsonConvert.SerializeObject(listquizid),
                ProfileId = 7,
                Status = true,
            };
            _context.Collections.Add(newCollection);
            await _context.SaveChangesAsync();
            return Json(new { status = true});
        }
        public async Task<IActionResult> UpdateDataCollection(int id,string name, string img, List<int> listquizid)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(img) || listquizid.Count < 1)
            {
                return Json(new { status = false });
            }
            var findCollection = await _context.Collections.FindAsync(id);
            if (findCollection == null) {
                return Json(new { status = false });
            }

            findCollection.Topic = name;
            findCollection.ImageCover = img;
            findCollection.ListEduQuizId = JsonConvert.SerializeObject(listquizid);
    
            await _context.SaveChangesAsync();
            return Json(new { status = true });
        }
        public async Task<IActionResult> GetDataDetail(int id, int flag)
        {
            object checkdata = null;

            if (flag == 1)
            {
                checkdata = await _context.Themes.FindAsync(id);
            }
            else
            {
                checkdata = await _context.Musics.FindAsync(id);
            }
            if (checkdata == null)
            {
                return Json(new { status = false });
            }
            return Json(new { status = true, data = checkdata });
        }
        #endregion
    }
}
