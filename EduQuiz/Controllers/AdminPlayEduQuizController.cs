using EduQuiz.DatabaseContext;
using EduQuiz.Models.EF;
using EduQuiz.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QRCoder;
using System.IdentityModel.Tokens.Jwt;

namespace EduQuiz.Controllers
{
    [CustomAuthorize("User")]
    public class AdminPlayEduQuizController : Controller
    {
        private readonly EduQuizDBContext _context;
        public AdminPlayEduQuizController(EduQuizDBContext context) { 
            _context = context;
        }
        [Route("playmode")]
        public async Task<IActionResult> Index(Guid quizId)
        {
            var getquiz = await _context.EduQuizs.FirstOrDefaultAsync(f => f.Uuid == quizId);
            if (getquiz == null)
            {
                return RedirectToAction("Index","HomeDashboard");
            }
            ViewBag.Quiz= getquiz;
            return View();
        }
    
        [Route("play")]
        public async Task<IActionResult> Lobby(Guid quizId)
        {
            var getquiz = await _context.EduQuizs.Include(p => p.Theme).FirstOrDefaultAsync(f => f.Uuid == quizId);
            if (getquiz == null)
            {
                return RedirectToAction("Index", "HomeDashboard");
            }
            ViewBag.Quiz = getquiz;
            return View();
        }

        public async Task<IActionResult> GenerateQRCode(int quizid,string title)
        {
            var authCookie = Request.Cookies["acToken"];
            int hostUserId = 0;
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                // Sử dụng các giá trị trong logic của bạn
                hostUserId = int.Parse(userId ?? "1");
            }
            string pin = await GeneratePin();  //Tạo mã PIN duy nhất
            var getquiz = await _context.EduQuizs
                .Where(q =>q.Id == quizid)
                .Include(x=>x.Questions)
                .ThenInclude(x=>x.Choices)
                .SingleOrDefaultAsync();
            var checkSnapshot = await _context.EduQuizSnapshots
                .SingleOrDefaultAsync(s => s.CreatedAt == getquiz.UpdateAt && s.EduQuizId == getquiz.Id);
            EduQuizSnapshot newSnapshot = null;
            if (checkSnapshot == null) {
                 newSnapshot = new EduQuizSnapshot
                {
                    EduQuizId = getquiz.Id,
                    Title = getquiz.Title,
                    Uuid = Guid.NewGuid(),
                    ImageCover = getquiz.ImageCover,
                    Description = getquiz.Description,
                    Type = getquiz.Type,
                    Visibility = getquiz.Visibility,
                    ThemeId = getquiz.ThemeId,
                    MusicId = getquiz.MusicId,
                    CreatedAt = getquiz.UpdateAt,
                    UserId = hostUserId,
                    Questions = new List<QuestionSnapshot>()
                };
                // Sao chép các câu hỏi và lựa chọn
                foreach (var originalQuestion in getquiz.Questions)
                {
                    var newQuestion = new QuestionSnapshot
                    {
                        QuestionText = originalQuestion.QuestionText,
                        TypeQuestion = originalQuestion.TypeQuestion,
                        TypeAnswer = originalQuestion.TypeAnswer,
                        Time = originalQuestion.Time,
                        PointsMultiplier = originalQuestion.PointsMultiplier,
                        Image = originalQuestion.Image,
                        ImageEffect = originalQuestion.ImageEffect,
                        Choices = new List<ChoiceSnapshot>()
                    };

                    if (originalQuestion.Choices?.Count > 0)
                    {
                        var newChoices = originalQuestion.Choices.Select(choice => new ChoiceSnapshot
                        {
                            Question = newQuestion, // Gán trực tiếp vào Question mới
                            Answer = choice.Answer,
                            IsCorrect = choice.IsCorrect,
                            DisplayOrder = choice.DisplayOrder
                        }).ToList();

                        newQuestion.Choices = newChoices; // Gán danh sách Choices vào Question
                    }

                    newSnapshot.Questions.Add(newQuestion);
                }

                // Lưu EduQuiz mới vào cơ sở dữ liệu
                _context.EduQuizSnapshots.Add(newSnapshot);
                await _context.SaveChangesAsync();

                var newQuestionIds = newSnapshot.Questions.Select(q => q.Id).ToList();
                newSnapshot.OrderQuestion = JsonConvert.SerializeObject(newQuestionIds);
            }

            var quizSession = new QuizSession
            {
                EduQuizId = quizid,
                HostUserId = hostUserId,
                Pin = pin,
                StartTime = DateTime.Now,
                Title = title,
                IsActive = true,
                IsWaitingRoom = true, // Đánh dấu là sảnh chờ,
                IsShowQuestionAndAnswer = true,
                EduQuizSnapshotId = checkSnapshot == null? newSnapshot.Id : checkSnapshot.Id
            };
            _context.QuizSessions.Add(quizSession);
            await _context.SaveChangesAsync();
            string url = $"{Request.Scheme}://{Request.Host}{Url.Action("JoinGame", "UserPlayEduQuiz", new { pin = pin })}";
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            using (var bitmap = qrCode.GetGraphic(20))
            {
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    var qrBlob = ms.ToArray();
                    return Json(new
                    {
                        pin = pin,
                        qrCodeBlob = Convert.ToBase64String(qrBlob),
                        idquizsession = quizSession.Id
                    });
                }
            }
        }
        public async Task<string> GeneratePin()
        {
            Random random = new Random();
            string pin;
            bool pinExists;

            do
            {
                pin = random.Next(100000, 999999).ToString();  // Tạo mã PIN 6 chữ số
                pinExists = await _context.QuizSessions.AnyAsync(q => q.Pin == pin && q.IsActive);
            } while (pinExists);  // Lặp lại cho đến khi tìm được mã PIN không trùng

            return pin;  // Trả về mã PIN duy nhất
        }
    }
}
