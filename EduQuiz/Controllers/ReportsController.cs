using EduQuiz.DatabaseContext;
using EduQuiz.Helper;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EduQuiz.Controllers
{
    [CustomAuthorize]
    public class ReportsController : Controller
    {
        private readonly EduQuizDBContext _context;
        public ReportsController(EduQuizDBContext context)
        {
            _context = context;
        }
        [Route("reports/live")]
        public async Task <IActionResult> Index()
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");

                var quizSessions = await _context.QuizSessions
                    .Where(q => q.HostUserId == iduser && q.TypeQuizSession == 0)
                    .Select(q => new ReportData
                    {
                        Pin = q.Pin,
                        QuizSessionId = q.Id,
                        Title = q.EduQuiz.Title,     
                        IsActive = q.IsActive,
                        StartTime = q.StartTime,
                        EndTime = q.EndTime,
                        PlayerCount = _context.PlayerSessions
                            .Where(p => p.QuizSessionId == q.Id)
                            .Count()  
                    })
                    .ToListAsync();
                return View(quizSessions);
            }
            return View();
        }
        [Route("reports/assignment")]
        public async Task <IActionResult> ReportAssignment()
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");

                var quizSessions = await _context.QuizSessions
                    .Where(q => q.HostUserId == iduser && q.TypeQuizSession == 1)
                    .Select(q => new ReportData
                    {
                        Pin =q.Pin,
                        QuizSessionId = q.Id,
                        Title = q.EduQuiz.Title,
                        IsActive = q.IsActive,
                        StartTime = q.StartTime,
                        EndTime = q.EndTime,
                        PlayerCount = _context.PlayerSessions
                            .Where(p => p.QuizSessionId == q.Id)
                            .Count()
                    })
                    .ToListAsync();
                return View(quizSessions);
            }
            return View();
        }

        [Route("reports/detail/{slug}")]
        public async Task<IActionResult> ReportDetail(string slug)
        {
            var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
            var parts = decodedSlug.Split('-');
            var id = int.Parse(parts[0]);
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                var quizSessionData = await _context.QuizSessions
                    .Where(q => q.Id == id)
                    .Include(q => q.HostUser)
                    .Select(q => new
                    {
                        QuizSession = q,
                        HostUsername = q.HostUser.Username,
                        Type = q.TypeQuizSession == 0 ? "Trực tiếp" : "Bài chỉ định",
                        StartTime = q.StartTime ?? DateTime.Now,
                        EndTime = q.EndTime ?? DateTime.Now,
                        EduQuizId = q.EduQuizId,
                        Title = q.Title,
                        PlayerSessions = _context.PlayerSessions
                            .Where(ps => ps.QuizSessionId == q.Id)
                            .ToList(),
                        Questions = _context.Questions
                            .Where(qst => qst.EduQuizId == q.EduQuizId)
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (quizSessionData != null)
                {
                    var playerCount = quizSessionData.PlayerSessions.Count;
                    var questionCount = quizSessionData.Questions.Count;

    
                    var playerAnswers = await _context.PlayerAnswers
                        .Where(pa => pa.PlayerSession.QuizSessionId == quizSessionData.QuizSession.Id)
                        .ToListAsync();

         
                    var totalCorrectAnswers = playerAnswers.Count(pa => pa.IsCorrect);

                    var reportDetail = new ReportDetail
                    {
                        NameHost = quizSessionData.HostUsername,
                        QuizSessionId = quizSessionData.QuizSession.Id,
                        Type = quizSessionData.Type,
                        StartTime = StringHelper.ConvertDateTimeToCustomString(quizSessionData.StartTime),
                        Title = quizSessionData.Title,
                        PlayerCount = playerCount,
                        QuestionCount = questionCount,
                        TimeSession = Math.Round(StringHelper.CalculateSessionDurationInMinutes(quizSessionData.StartTime, quizSessionData.EndTime), 2),
                        TotalAccuracy = playerCount > 0 && questionCount > 0
                            ? (int?)Math.Ceiling((double)totalCorrectAnswers / (questionCount * playerCount) * 100)
                            : null,
                        QuestionDifficults = new List<ReportQuestionDifficult>(),
                        PlayersBelowAvg = new List<ReportBelowAvg>(),
                        PlayersNotFinish = new List<ReportNotFinish>()
                    };

                    
                    foreach (var question in quizSessionData.Questions)
                    {
                        var correctAnswersCount = playerAnswers.Count(pa => pa.QuestionId == question.Id && pa.IsCorrect);
                        var accuracy = (double)correctAnswersCount / playerCount;

                        if (accuracy < 0.35)
                        {
                            var averageTime = playerAnswers
                                .Where(pa => pa.QuestionId == question.Id)
                                .Average(pa => pa.TimeTaken);
                            var order = await _context.QuizSessionQuestions
                                .Where(o => o.QuizSessionId == quizSessionData.QuizSession.Id && o.QuestionId == question.Id)
                                .Select(o => o.Order)
                                .FirstOrDefaultAsync();

                            reportDetail.QuestionDifficults.Add(new ReportQuestionDifficult
                            {
                                QuestionId = question.Id,
                                TypeQuestion = question.TypeQuestion switch
                                {
                                    "quiz" => "Câu đố",
                                    "true_false" => "Đúng sai",
                                    _ => "Nhập đáp án"
                                },
                                QuestionTitle = question.QuestionText,
                                QuestionImage = question.Image,
                                Order = order,
                                Accuracy = (int)Math.Ceiling(accuracy * 100),
                                AverageTime = (float)Math.Round(averageTime, 2)
                            });
                        }
                    }

                   
                    foreach (var playerSession in quizSessionData.PlayerSessions)
                    {
                        var correctAnswers = playerAnswers.Count(pa => pa.PlayerSessionId == playerSession.Id && pa.IsCorrect);
                        var playerAccuracy = (double)correctAnswers / questionCount;

                        if (playerAccuracy < 0.5)
                        {
                            reportDetail.PlayersBelowAvg.Add(new ReportBelowAvg
                            {
                                PlayerId = playerSession.Id,
                                PlayerName = playerSession.Nickname,
                                Accuracy = (int)Math.Ceiling(playerAccuracy * 100)
                            });
                        }

                        var unansweredQuestions = quizSessionData.Questions.Count(q =>
                            !playerAnswers.Any(pa => pa.QuestionId == q.Id && pa.PlayerSessionId == playerSession.Id));

                        if (unansweredQuestions > 0)
                        {
                            reportDetail.PlayersNotFinish.Add(new ReportNotFinish
                            {
                                PlayerId = playerSession.Id,
                                PlayerName = playerSession.Nickname,
                                CountNotAnswer = unansweredQuestions
                            });
                        }
                    }

                    return View(reportDetail);
                }
            }
            return View();
        }
        [Route("reports/detail/{slug}/podium")]
        public async Task<IActionResult> PodiumQuizSession(string slug)
        {
            var authCookie = Request.Cookies["acToken"];
            if (authCookie != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(authCookie);
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                var iduser = int.Parse(userId ?? "1");
                var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
                var parts = decodedSlug.Split('-');
                var id = int.Parse(parts[0]);
                var getResultPodium = await _context.PlayerSessions
                    .Where(p=>p.QuizSessionId ==id)
                    .Select(n => new ReportPodium
                    {
                        Accessory = n.Accessory,
                        AvatarUrl = n.AvatarUrl,
                        Nickname = n.Nickname,
                        TotalScore = n.TotalScore
                    }).OrderByDescending(p=>p.TotalScore)
                    .Take(3)
                    .ToListAsync();
                return View(getResultPodium);
            }
            return View();
        }
        #region handle
        #endregion
    }
}
