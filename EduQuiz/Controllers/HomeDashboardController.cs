using EduQuiz.DatabaseContext;
using EduQuiz.Helper;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using EduQuiz.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using BcryptNet = BCrypt.Net.BCrypt;

namespace EduQuiz.Controllers
{
    [CustomAuthorize("User")]
    public class HomeDashboardController : Controller
    {
        private readonly EduQuizDBContext _context;
        private readonly IConfiguration _config; 
        private readonly CookieAuth _cookieAuth;
        public HomeDashboardController(EduQuizDBContext context,IConfiguration config, CookieAuth cookieAuth)
        {
            _context = context;
            _config = config;
            _cookieAuth = cookieAuth;
        }
        [Route("home")]
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
            var avatar = jwtToken.Claims.FirstOrDefault(c => c.Type == "Avatar")?.Value;
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
            var iduser = int.Parse(userId ?? "1");

            var user = await _context.Users.FindAsync(iduser);

            var listGroupbyUserOwner = await _context.Groups
                .Where(g => g.UserId == iduser)
                .Select(n => new HomeGroupView
                {
                    Id = n.Id,
                    Name = n.Name,
                    SumMember = _context.GroupMembers.Count(c => c.GroupId == n.Id),
                    Uuid = n.Uuid,
                }).ToListAsync();

            var listGroupbyUserJoin = await _context.GroupMembers
              .Where(gm => gm.UserId == iduser)
              .Join(_context.Groups, gm => gm.GroupId, g => g.Id, (gm, g) => new HomeGroupView
              {
                  Id = g.Id,
                  Name = g.Name,
                  SumMember = _context.GroupMembers.Count(c => c.GroupId == g.Id),
                  IsHost = g.UserId == iduser,
                  Uuid = g.Uuid,
              }).ToListAsync();

            var groupIds = listGroupbyUserJoin.Select(g => g.Id).ToList();

            var listAssignmentbyUser = await (from ag in _context.AssignmentGroups
                                              join q in _context.QuizSessions on ag.QuizSessionId equals q.Id
                                              join e in _context.EduQuizs on ag.EduQuizId equals e.Id
                                              join u in _context.Users on e.UserId equals u.Id
                                              where groupIds.Contains(ag.GroupId ?? 0) && DateTime.Now < q.EndTime
                                              select new HomeAssignmentView
                                              {
                                                  Id = ag.Id,
                                                  Pin = q.Pin,
                                                  Deadline = CalculateHelper.ConvertDeadline(q.EndTime ?? DateTime.Now),
                                                  Image = e.ImageCover,
                                                  Title = e.Title,
                                                  SumQuestion = _context.Questions.Count(q => q.EduQuizId == e.Id),
                                                  UserName = u.Username
                                              }).ToListAsync();

            var listEduQuizbyUser = await _context.EduQuizs
                .Where(g => g.UserId == iduser && g.Status)
                .Select(n => new HomeEduQuizView
                {
                    Id = n.Id,
                    Uuid = n.Uuid,
                    Title = n.Title == ""? "Chưa đặt tên" : n.Title,
                    Image = n.ImageCover,
                    Type= n.Type ?? 0,
                    Avatar = avatar,
                    UserName = username,
                    SumPlay = _context.QuizSessions.Count(p => p.EduQuizId == n.Id),
                    SumQuestion = _context.Questions.Count(q => q.EduQuizId == n.Id)
                }).Take(5).ToListAsync();

            var userFavorites = JsonConvert.DeserializeObject<List<InterestUser>>(user.Favorite)?.Select(f => f.id).ToList();
            var favoriteEduQuizQuery = _context.EduQuizs
                .Where(n => userFavorites.Contains(n.TopicId ?? 1) && n.Visibility && n.Type == 1 && n.Status && n.UserId != 8) // Chỉ lấy các EduQuiz có TopicId trong danh sách yêu thích
                .Include(n => n.User)
                .Select(n => new HomeEduQuizView
                {
                    Id = n.Id,
                    Uuid = n.Uuid,
                    Title = n.Title,
                    Image = n.ImageCover,
                    Avatar = n.User.ProfilePicture,
                    UserName = n.User.Username,
                    SumPlay = _context.QuizSessions.Count(p => p.EduQuizId == n.Id),
                    SumQuestion = _context.Questions.Count(q => q.EduQuizId == n.Id)
                })
                .OrderByDescending(n => n.SumPlay)
                .Take(5);

            List<HomeEduQuizView> listEduQuizHot;

            if (userFavorites == null || userFavorites.Count == 0)
            {
                // Nếu user chưa có sở thích, lấy top 5 EduQuiz được chơi nhiều nhất
                listEduQuizHot = await _context.EduQuizs
                    .Where(n=>n.Visibility && n.Type == 1 && n.Status && n.UserId != 8)
                    .Include(n => n.User)
                    .Select(n => new HomeEduQuizView
                    {
                        Id = n.Id,
                        Uuid = n.Uuid,
                        Title = n.Title,
                        Image = n.ImageCover,
                        Avatar = n.User.ProfilePicture,
                        UserName = n.User.Username,
                        SumPlay = _context.QuizSessions.Count(p => p.EduQuizId == n.Id),
                        SumQuestion = _context.Questions.Count(q => q.EduQuizId == n.Id)
                    })
                    .OrderByDescending(n => n.SumPlay)
                    .Take(5)
                    .ToListAsync();
            }
            else
            {
                // Nếu có sở thích, lấy theo sở thích và bổ sung nếu thiếu
                listEduQuizHot = await favoriteEduQuizQuery.ToListAsync();

                if (listEduQuizHot.Count < 5)
                {
                    var topEduQuizHot = await _context.EduQuizs
                        .Include(n => n.User)
                        .Where(n => !userFavorites.Contains(n.TopicId ?? 1) && n.Visibility && n.Status && n.UserId != 8) 
                        .Select(n => new HomeEduQuizView
                        {
                            Id = n.Id,
                            Uuid = n.Uuid,
                            Title = n.Title,
                            Image = n.ImageCover,
                            Avatar = n.User.ProfilePicture,
                            UserName = n.User.Username,
                            SumPlay = _context.QuizSessions.Count(p => p.EduQuizId == n.Id),
                            SumQuestion = _context.Questions.Count(q => q.EduQuizId == n.Id)
                        })
                        .OrderByDescending(n => n.SumPlay)
                        .Take(5 - listEduQuizHot.Count) // Bổ sung cho đủ 5 mục
                        .ToListAsync();

                    listEduQuizHot.AddRange(topEduQuizHot);
                }
            }
            var listReportbyUser = await _context.QuizSessions
                .Where(r => r.HostUserId == iduser)
                .Select(n => new HomeReportView
                {
                    Id = n.Id,
                    ReportDate = StringHelper.ConvertDateTimeToCustomString(n.StartTime ?? DateTime.Now),
                    Title = n.Title,
                    Pin = n.Pin,
                }).OrderByDescending(n => n.Id).Take(5).ToListAsync();

            var view = new HomeViewModel
            {
                ListAssignment = listAssignmentbyUser,
                ListEduQuiz = listEduQuizHot,
                ListEduQuizOwner = listEduQuizbyUser,
                ListGroupJoin = listGroupbyUserJoin.Where(n => n.IsHost == false).ToList(),
                ListGroupOwner = listGroupbyUserOwner,
                User = user,
                ListReport = listReportbyUser
            };

            return View(view);
        }


        [Route("user/profile")]
        public async Task<IActionResult> SettingInfo()
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                var user = await _context.Users.FindAsync(iduser);
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
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                var user = await _context.Users.FindAsync(iduser);
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
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                var user = await _context.Users.FindAsync(iduser);
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
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                var user = await _context.Users.FindAsync(iduser);
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
                var authCookie = Request.Cookies["acToken"];
                int iduser = 0;
                if (authCookie != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                    var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                    iduser = int.Parse(userId ?? "1");
                }else
                {
                    return Json(new { result = "FAIL", msg = "Vui lòng đăng nhập lại" });
                }
                var user = await _context.Users.FindAsync(iduser);
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
                var acToken = _cookieAuth.GenerateToken(user);

                HttpContext.Response.Cookies.Append("acToken", acToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(15)
                });

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
