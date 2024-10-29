using EduQuiz.DatabaseContext;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using EduQuiz.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace EduQuiz.Controllers
{
    [CustomAuthorize]
    public class DetailController : Controller
    {
        private readonly EduQuizDBContext _context;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public DetailController(EduQuizDBContext context,IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }
        [Route("detail/{id:guid}")]
        public async Task<IActionResult> Index(Guid id)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var iduser = int.Parse(userId ?? "1");
            var check = await _context.EduQuizs
                   .Include(e => e.Questions)
                   .ThenInclude(q => q.Choices)
                   .FirstOrDefaultAsync(d => d.Uuid == id && d.Type == 1);

            if (check == null || (!check.Visibility && check.UserId != iduser))
            {
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(referer))
                {
                    return Redirect(referer);
                }
                return RedirectToAction("Error404", "Home");
            }

            List<int> orderQuestion = JsonConvert.DeserializeObject<List<int>>(check.OrderQuestion) ?? new List<int>();

            var getdata = new Models.EduQuizData
            {
                Id = check.Id,
                Uuid = check.Uuid,
                Description = check.Description,
                Title = check.Title,
                ImageCover = check.ImageCover,
                Type = check.Type ?? 0,
                Visibility = check.Visibility,
                ThemeId = check.ThemeId ?? 0,
                MusicId = check.MusicId ?? 0,
                UserId = check.UserId ?? 0,
                UpdateAt = check.UpdateAt,
                Questions = check.Questions.Select(q => new QuestionData
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    TypeQuestion = q.TypeQuestion,
                    TypeAnswer = q.TypeAnswer ?? 0,
                    Time = q.Time ?? 0,
                    PointsMultiplier = q.PointsMultiplier ?? 0,
                    Image = q.Image,
                    ImageEffect = q.ImageEffect,
                    Choices = q.Choices.OrderBy(c => c.DisplayOrder).Select(c => new ChoiceData
                    {
                        Id = c.Id,
                        Answer = c.Answer,
                        IsCorrect = c.IsCorrect,
                        DisplayOrder = c.DisplayOrder
                    }).ToList()
                }).ToList()
            };

            var orderLookup = orderQuestion.Select((id, index) => new { id, index })
                .ToDictionary(x => x.id, x => x.index);

            getdata.Questions = getdata.Questions
                .OrderBy(q => orderLookup.GetValueOrDefault(q.Id, int.MaxValue))
                .ToList();

            var getInfoUser = await _context.Users.FindAsync(getdata.UserId);
            var quizSessions = await _context.QuizSessions
                .Where(q => q.EduQuizId == check.Id)
                .ToListAsync();
            var sessionIds = quizSessions.Select(q => q.Id).ToList();
            var playerCount = await _context.PlayerSessions
                .Where(p => sessionIds.Contains(p.QuizSessionId))
                .CountAsync();
            var checkFavorite = await _context.EduQuizFavorite.FirstOrDefaultAsync(f => f.EduQuizId == getdata.Id && f.UserId == iduser);
            if (getInfoUser != null)
            {
                ViewBag.Data = JsonConvert.SerializeObject(new
                {
                    UserCurrent = iduser,
                    UserName = getInfoUser.Username,
                    Avatar = getInfoUser.ProfilePicture,
                    QuizSessionCount = quizSessions.Count,
                    PlayerCount = playerCount,
                    IsFavorite = checkFavorite!= null
                });
            }
            return View(getdata);
   
        }
        #region handle
        public async Task<IActionResult> SaveFavoriteEduQuiz(int idquiz)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authCookie);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value;
            var iduser = int.Parse(userId ?? "1");
            var check = await _context.EduQuizFavorite.FirstOrDefaultAsync(f => f.EduQuizId == idquiz && f.UserId == iduser);
          
            if (check == null) {
                var geteduquiz = await _context.EduQuizs.Where(e=>e.Id == idquiz).Include(e => e.User).FirstOrDefaultAsync();
                dynamic privacySettings = JsonConvert.DeserializeObject<dynamic>(geteduquiz.User.PrivacySettings);
                if (privacySettings["FavoriteEduQuiz"] == true)
                {
                    SendEmail(
                        geteduquiz.User.Email,
                        $"EduQuiz {geteduquiz.Title} vừa được yêu thích",
                        geteduquiz.User.Username,
                        $"{userName} vừa yêu thích EduQuiz '{geteduquiz.Title}' của bạn",
                        geteduquiz.Title,
                        "Chi tiết EduQuiz",
                        $"/detail/{geteduquiz.Uuid}"
                    );
                }
                var newEduQuizFavorite = new EduQuizFavorite
                {
                    EduQuizId = idquiz,
                    UserId = iduser,
                };
                _context.EduQuizFavorite.Add(newEduQuizFavorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.EduQuizFavorite.Remove(check);
                await _context.SaveChangesAsync();
            }
            return Json(new { result = "PASS" });
        }
        private void SendEmail(string recipientEmail, string subject, string username, string msgtitle, string groupname, string type, string redirecturl)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/src/templates", "EmailTemplate.html");
            string emailBody;

            try
            {
                emailBody = System.IO.File.ReadAllText(templatePath);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi đọc file", ex);
            }
            var domain = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

            emailBody = emailBody.Replace("{domain}", domain);
            emailBody = emailBody.Replace("{type}", type);
            emailBody = emailBody.Replace("{username}", username);
            emailBody = emailBody.Replace("{msgtitle}", msgtitle); // pingvocuc333 vừa chia sẻ EduQuiz của họ "Tăng cường vốn từ vựng"" với bạn!
            emailBody = emailBody.Replace("{groupname}", groupname);
            emailBody = emailBody.Replace("{redirecturl}", domain + redirecturl);

            // Gửi email sử dụng dịch vụ email
            _emailService.SendEmail(recipientEmail, subject, emailBody);
        }
        #endregion
    }
}
