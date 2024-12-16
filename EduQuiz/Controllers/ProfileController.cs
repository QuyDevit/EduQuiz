using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EduQuiz.Controllers
{
    [CustomAuthorize("User")]
    public class ProfileController : Controller
    {
        private readonly EduQuizDBContext _context;
        public ProfileController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("/profiles/manage")]
        public async Task <IActionResult> Index()
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
            var getuser = await _context.Users.FindAsync(iduser);
            var getprofile= await _context.Profile.FirstOrDefaultAsync(p=>p.UserId == iduser);
            List<EduQuizProfile> listeduquiz = new List<EduQuizProfile>();
            if (getprofile != null) {
                List<int> quizid = JsonConvert.DeserializeObject<List<int>>(getprofile.ListEduQuizTop);
                foreach (var id in quizid)
                {
                    var eduQuizProfile = await _context.EduQuizs
                    .Where(n => n.Id == id)
                    .Select(n => new EduQuizProfile
                    {
                        Id = n.Id,
                        Image = n.ImageCover,
                        Name = n.Title,
                    })
                    .FirstOrDefaultAsync();
                    if (eduQuizProfile != null)
                    {
                        listeduquiz.Add(eduQuizProfile);
                    }
                }
            }
            var view = new ProfileViewModel
            {
                User = getuser,
                Profile = getprofile,
                ListEduQuizProfile = listeduquiz,
            };
            return View(view);
        }
        [Route("/profiles/{id:guid}")]
        public async Task<IActionResult> ProfilePage(Guid id)
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

            var infoProfile = await _context.Profile
                .Where(c => c.Uuid == id)
                .Select(c => new { c.Id, c.Image, c.TitlePage, c.Status, c.UserId, c.ListEduQuizTop })
                .FirstOrDefaultAsync();

            if (infoProfile == null || (!infoProfile.Status && infoProfile.UserId != iduser && iduser != 8))
            {
                return RedirectToAction("Error404", "Home");
            }

            var getuser = await _context.Users
                .Where(n => n.Id == infoProfile.UserId)
                .Select(n => new { n.ProfilePicture, n.Email, n.Username })
                .FirstOrDefaultAsync();

            var userCurrent = await _context.Users
                .Where(n => n.Id == iduser)
                .Select(n => new { n.SubscriptionEndDate,n.Id })
                .FirstOrDefaultAsync();
            var subsType = (userCurrent.SubscriptionEndDate.HasValue && userCurrent.SubscriptionEndDate > DateTime.Now) ? "vip" : "free";
            if(subsType != "vip")
            {
                var checkvip = await _context.GroupMembers
                .Where(gm => gm.UserId == userCurrent.Id)
                .Include(gm => gm.Group)
                .ToListAsync();

                if (checkvip.Any(gm => gm.Group.SubscriptionEndDate.HasValue && gm.Group.SubscriptionEndDate > DateTime.Now))
                {
                    subsType = "vip";
                }
            }
            List<CollectionItem> listcollection = new List<CollectionItem>();

            if (infoProfile.UserId == 8)
            {
                var getcollection = await _context.Collections.ToListAsync();
                foreach (var item in getcollection)
                {
                    var eduQuizIdscolletion = JsonConvert.DeserializeObject<List<int>>(item.ListEduQuizId);

                    var eduQuizItemsbyCollection = await _context.EduQuizs
                        .Where(n => eduQuizIdscolletion.Contains(n.Id))
                        .Select(eduquizItem => new EduQuizItem
                        {
                            Image = eduquizItem.ImageCover,
                            Title = eduquizItem.Title,
                            Type = "Eduquiz+",
                            UserName = getuser.Username,
                            Uuid = eduquizItem.Uuid,
                            SumQuestion = _context.Questions.Count(q => q.EduQuizId == eduquizItem.Id)
                        }).ToListAsync();

                    listcollection.Add(new CollectionItem
                    {
                        Topic = item.Topic,
                        EduQuizCollection = eduQuizItemsbyCollection
                    });
                }
            }

            var eduquizstop = JsonConvert.DeserializeObject<List<int>>(infoProfile.ListEduQuizTop);
            var eduquizByUser = await _context.EduQuizs
                .Where(n => eduquizstop.Contains(n.Id))
                .Select(n => new { n.Id, n.ImageCover, n.Title, n.Uuid })
                .ToListAsync();

            var eduQuizIds = eduquizByUser.Select(e => e.Id).ToList();
            List<EduQuizItem> eduQuizItems = new List<EduQuizItem>();

            if (infoProfile.UserId != 8)
            {
                eduQuizItems = eduquizByUser.Select(eduquizItem => new EduQuizItem
                {
                    Image = eduquizItem.ImageCover,
                    Title = eduquizItem.Title,
                    Type = "Miễn phí",
                    UserName = getuser.Username,
                    Uuid = eduquizItem.Uuid,
                    SumQuestion = _context.Questions.Count(q => q.EduQuizId == eduquizItem.Id)
                }).ToList();
            }

            var sumPlay = await _context.QuizSessions
                .CountAsync(n => eduQuizIds.Contains(n.EduQuizId));

            var sumPlayerPlay = await _context.PlayerSessions
                .CountAsync(n => _context.QuizSessions
                    .Any(q => q.Id == n.QuizSessionId && eduQuizIds.Contains(q.EduQuizId)));

            var isFollow = await _context.Follows
                .AnyAsync(n => n.UserId == iduser && n.ProfileId == infoProfile.Id);

            var view = new ProfilePage
            {
                Avatar = getuser.ProfilePicture,
                Email = getuser.Email,
                ImageCover = infoProfile.Image,
                PageTitle = infoProfile.TitlePage,
                SumEduQuiz = eduquizByUser.Count,
                SumPlay = sumPlay,
                SumPlayerPlay = sumPlayerPlay,
                ListEduQuizItem = eduQuizItems,
                IsFollow = isFollow,
                IsHost = infoProfile.UserId == iduser,
                UserCurrentSubscriptionType = subsType,
                ListCollection = listcollection
            };

            return View(view);
        }

        [Route("/profiles/{id:guid}/about")]
        public async Task<IActionResult> ProfilePageAbout(Guid id)
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
          
            var infoProfile = await _context.Profile
                .Where(c => c.Uuid == id)
                .Select(c => new { c.Id, c.Image, c.TitlePage ,c.DescriptionPage ,c.LinkZalo,c.LinkYoutube,c.LinkFacebook,c.LinkInstagram,c.UserId })
                .FirstOrDefaultAsync();
            if (infoProfile == null)
            {
                return RedirectToAction("Error404", "Home");
            }
            var getuser = await _context.Users.Where(n => n.Id == infoProfile.UserId)
            .Select(n => new { n.ProfilePicture, n.Email, n.Username })
            .FirstOrDefaultAsync();
      
            var eduquizByUser = await _context.EduQuizs
                .Where(n => n.UserId == infoProfile.UserId)
                .Select(n => new { n.Id, n.ImageCover, n.Title, n.Uuid })
                .ToListAsync();

            var eduQuizIds = eduquizByUser.Select(e => e.Id).ToList();

            var sumPlay = await _context.QuizSessions
                .CountAsync(n => eduQuizIds.Contains(n.EduQuizId));

            var sumPlayerPlay = await _context.PlayerSessions
                .CountAsync(n => _context.QuizSessions
                    .Any(q => q.Id == n.QuizSessionId && eduQuizIds.Contains(q.EduQuizId)));

            var isFollow = await _context.Follows
                .FirstOrDefaultAsync(n => n.UserId == iduser && n.ProfileId == infoProfile.Id);
            var view = new ProfilePage
            {
                Avatar = getuser.ProfilePicture,
                Email = getuser.Email,
                ImageCover = infoProfile.Image,
                PageTitle = infoProfile.TitlePage,
                SumEduQuiz = eduquizByUser.Count,
                LinkZalo = infoProfile.LinkZalo,
                LinkYoutube = infoProfile.LinkYoutube,
                LinkFacebook = infoProfile.LinkFacebook,
                LinkInstagram = infoProfile.LinkInstagram,
                SumPlay = sumPlay,
                PageDescription = infoProfile.DescriptionPage,
                SumPlayerPlay = sumPlayerPlay,
                IsFollow = isFollow != null,
                IsHost = infoProfile.UserId == iduser,
            };
            return View(view);
        }
        #region handle
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
        public async Task<IActionResult> SaveImgProfilePage([FromForm] IFormFile image)
        {
            try
            {
                if (image != null && image.Length > 0)
                {
                    var authCookie = Request.Cookies["acToken"];
                    if (authCookie == null)
                    {
                        return Json(new { status = false });
                    }
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                    var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                    var iduser = int.Parse(userId ?? "1");

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "src", "img", "page");

                    Directory.CreateDirectory(uploadsFolder);

                    var fileExtension = Path.GetExtension(image.FileName);
                    var uniqueFileName = $"profile_page_user_{iduser}" + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Lưu file lên server
                    await using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    return Json(new { status = true, url = $"/src/img/page/{uniqueFileName}" });
                }

                return Json(new { status = false });
            }
            catch (Exception ex)
            {
                return Json(new { status = false});
            }
        }
        public async Task<IActionResult> SaveProfile(string image, string zaloLink,string youtubeLink,string facebookLink
            ,string instagramLink,string title,string description,string donate,List<int> listeduquizid)
        {
            try
            {
                var authCookie = Request.Cookies["acToken"];
                if (authCookie == null)
                {
                    return Json(new { status = false });
                }
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                if (string.IsNullOrEmpty(image) || string.IsNullOrEmpty(title)|| string.IsNullOrEmpty(description) || listeduquizid.Count <= 0)
                {
                    return Json(new { status = false });
                }
                var checkProfile = await _context.Profile.FirstOrDefaultAsync(c => c.UserId == iduser);
                if (checkProfile == null)
                {
                    var newProfile = new Profile
                    {
                        DescriptionPage = description,
                        InfoDonate = donate == null? "" : donate,
                        LinkFacebook = facebookLink,
                        LinkInstagram = instagramLink,
                        LinkYoutube = youtubeLink,
                        Image = image,
                        LinkZalo = zaloLink,
                        TitlePage = title,
                        UserId = iduser,
                        ListEduQuizTop = JsonConvert.SerializeObject(listeduquizid)
                    };
                    _context.Profile.Add(newProfile);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    checkProfile.LinkYoutube = youtubeLink;
                    checkProfile.TitlePage = title;
                    checkProfile.DescriptionPage = description;
                    checkProfile.InfoDonate = donate;
                    checkProfile.LinkZalo = zaloLink;
                    checkProfile.LinkFacebook = facebookLink;
                    checkProfile.ListEduQuizTop = JsonConvert.SerializeObject(listeduquizid);
                    checkProfile.LinkInstagram = instagramLink;
                    checkProfile.Image = image;
                    await _context.SaveChangesAsync();
                }
                return Json(new { status = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { status = false, message = ex.Message });
            }
        }
        public async Task<IActionResult> FollowPage(Guid uuid)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return Json(new { status = false });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");
            var getidpage = await _context.Profile.Where(c => c.Uuid == uuid).Select(n => new{ n.Id}).FirstOrDefaultAsync();
            if (getidpage == null)
            {
                return Json(new { status = false });
            }
            var checkFollow = await _context.Follows.FirstOrDefaultAsync(f=>f.ProfileId == getidpage.Id && f.UserId == iduser);
            if(checkFollow == null)
            {
                var newFollow = new Follow
                {
                    UserId = iduser,
                    ProfileId = getidpage.Id,
                };
                _context.Follows.Add(newFollow);
            }
            else
            {
                _context.Follows.Remove(checkFollow);
            }
            await _context.SaveChangesAsync();
            return Json(new { status = true });
        }
        #endregion
    }
}
