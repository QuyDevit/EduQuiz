using EduQuiz.DatabaseContext;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.Controllers
{
    public class UserPlayEduQuizController : Controller
    {
        private readonly EduQuizDBContext _context;
        public UserPlayEduQuizController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("pin")]
        public IActionResult Index()
        {
            return View();
        }
        [Route("join")]
        public IActionResult JoinGame(string pin)
        {
            var getQuizSession = _context.QuizSessions.FirstOrDefault(x => x.Pin == pin);
            if (getQuizSession == null)
            {
                return RedirectToAction("Index", "UserPlayEduQuiz");
            }
            ViewBag.QuizSession = getQuizSession;
            return View();
        }
        [Route("playquiz")]
        public async Task<IActionResult> PlayGame(string connectId)
        {
            var checkPlayer = await _context.PlayerSessions.Include(q => q.QuizSession).ThenInclude(p=>p.EduQuiz).ThenInclude(p=>p.Theme).FirstOrDefaultAsync(c=>c.ConnectionId == connectId);
            if (checkPlayer == null)
            {
                return RedirectToAction("Index", "UserPlayEduQuiz");
            }
            ViewBag.Player = checkPlayer;
            return View();
        }
        public async Task <IActionResult> CheckPinGame(string pin)
        {
            var checkpin = await _context.QuizSessions.FirstOrDefaultAsync(p => p.Pin == pin && p.IsActive == true);
            if (checkpin == null) {
                return Json(new { status = false });
            }
            return Json(new { status = true});

        }
        public async Task<IActionResult> SaveFeedback(FeedbackQuizSession data)
        {
            _context.FeedbackQuizSessions.Add(data);
            var result = await _context.SaveChangesAsync();
           
            return Json(new { status = result });

        }
    }
}
