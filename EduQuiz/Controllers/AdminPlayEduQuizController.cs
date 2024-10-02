using EduQuiz.DatabaseContext;
using EduQuiz.Models.EF;
using EduQuiz.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.IdentityModel.Tokens.Jwt;

namespace EduQuiz.Controllers
{
    [CustomAuthorize]
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
