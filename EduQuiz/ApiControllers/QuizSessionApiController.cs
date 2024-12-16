using EduQuiz.DatabaseContext;
using EduQuiz.Models.API;
using EduQuiz.Models.EF;
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
        [HttpPost("GetQuizSessionByUser")]
        public async Task<IActionResult> GetQuizSessionByUser([FromBody] ConnectIdRequest data)
        {
            if (string.IsNullOrEmpty(data.ConnectId))
            {
                return Json(new {msg = "ConnectId is require!" });
            }
            var checkPlayer = await (from p in _context.PlayerSessions 
                                     join q in _context.QuizSessions on p.QuizSessionId equals q.Id
                                     join e in _context.EduQuizSnapshots on p.QuizSession.EduQuizSnapshotId equals e.Id
                                     join t in _context.Themes on e.ThemeId equals t.Id 
                                     join m in _context.Musics on e.MusicId equals m.Id
                                     where p.ConnectionId == data.ConnectId
                                     select new
                                     {
                                         IdPlayer = p.Id,
                                         Theme = t.Source,
                                         Music = m.Source,
                                         Avatar =p.AvatarUrl,
                                         p.Accessory,
                                         p.Nickname,
                                         q.IsWaitingRoom,
                                         q.IsActive,
                                         q.IsShowAvatar,
                                         p.IsPlayer
                                     })
                                     .SingleOrDefaultAsync();
            return Json(checkPlayer);
        }
        [HttpPost("SendFeedback")]
        public async Task<IActionResult> SendFeedback([FromBody] FeedbackRequest data)
        {
            var newFeedback = new FeedbackQuizSession
            {
                Liked = data.Liked,
                PositiveFeeling = data.PositiveFeeling,
                QuizSessionId = data.QuizSessionId,
                PositiveLearningOutcome = data.PositiveLearningOutcome,
                CreatedDate = DateTime.UtcNow,
                Rating=data.Rating,
            };
            _context.FeedbackQuizSessions.Add(newFeedback);
            var result = await _context.SaveChangesAsync();

            return Json(new { status = result });

        }
    }
}
