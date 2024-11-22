using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.ApiControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizSessionApiController : Controller
    {
        private readonly EduQuizDBContext _context;
        public QuizSessionApiController(EduQuizDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> GetQuizSessionByUser(string connectId)
        {
            var checkPlayer = await _context.PlayerSessions
                .Include(q => q.QuizSession)
                .ThenInclude(p => p.EduQuiz)
                .ThenInclude(p => p.Theme)
                .FirstOrDefaultAsync(c => c.ConnectionId == connectId);
            return Json(checkPlayer);
        }
    }
}
