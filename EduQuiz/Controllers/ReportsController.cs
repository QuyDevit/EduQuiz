using EduQuiz.DatabaseContext;
using EduQuiz.Helper;
using EduQuiz.Models;
using EduQuiz.Models.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NuGet.Packaging.Signing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Numerics;
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
                    .OrderByDescending(q => q.Id)
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
            if (!IsBase64String(slug))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
            var parts = decodedSlug.Split('-');
            if (parts.Length == 0 || !int.TryParse(parts[0], out var id))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
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
                            .Where(ps => ps.QuizSessionId == q.Id).AsNoTracking()
                            .ToList(),
                        Questions = _context.Questions
                            .Where(qst => qst.EduQuizId == q.EduQuizId)
                            .Include(qst =>qst.Choices).AsNoTracking()
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

                    // Tổng tỷ lệ chính xác cho tất cả các câu hỏi
                    double totalAccuracySum = 0;

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
                        QuestionDifficults = new List<ReportQuestionDifficult>(),
                        PlayersBelowAvg = new List<ReportBelowAvg>(),
                        PlayersNotFinish = new List<ReportNotFinish>()
                    };

                    
                    foreach (var question in quizSessionData.Questions)
                    {
                        var correctAnswersCount = playerAnswers.Count(pa => pa.QuestionId == question.Id && pa.IsCorrect);
                        var totalCorrectAnswersForQuestion = question.Choices
                            .Where(a => a.QuestionId == question.Id && a.IsCorrect)
                            .Count();
                        double accuracy = 0;

                        if (question.TypeAnswer == 2)
                        {
                            accuracy = (double)correctAnswersCount / (playerCount * totalCorrectAnswersForQuestion);
                        }
                        else
                        {
                            accuracy = (double)correctAnswersCount / playerCount;
                        };
                        totalAccuracySum += accuracy;
                        if (accuracy < 0.35)
                        {
                            var questionAnswers = playerAnswers.Where(pa => pa.QuestionId == question.Id);
                            float averageTime = 0;
                            if (questionAnswers.Any())
                            {
                                averageTime = (float)Math.Round(questionAnswers.Average(pa => pa.TimeTaken), 2);
                            }
                            var order = await _context.QuizSessionQuestions
                                .Where(o => o.QuizSessionId == quizSessionData.QuizSession.Id && o.QuestionId == question.Id)
                                .Select(o => o.Order)
                                .FirstOrDefaultAsync();
                            if(order != 0)
                            {
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
                                    Accuracy = (int)Math.Round(accuracy * 100),
                                    AverageTime = (float)Math.Round(averageTime, 2)
                                });
                            }
                        }
                    }
                    reportDetail.QuestionDifficults = reportDetail.QuestionDifficults.OrderBy(r => r.Order).ToList();
                    reportDetail.TotalAccuracy = questionCount > 0
                       ? totalAccuracySum!=0? (int?)Math.Round(totalAccuracySum / questionCount * 100) : 0
                       : null;
                    foreach (var playerSession in quizSessionData.PlayerSessions)
                    {
                        var correctAnswers = playerAnswers.Count(pa => pa.PlayerSessionId == playerSession.Id && pa.IsCorrect);
                        double totalPlayerAccuracy = 0.0;

                        foreach (var question in quizSessionData.Questions)
                        {
                            var correctAnswersForQuestion = playerAnswers.Count(pa => pa.PlayerSessionId == playerSession.Id && pa.QuestionId == question.Id && pa.IsCorrect);
                            var totalCorrectAnswersForQuestion = question.Choices
                                .Where(a => a.QuestionId == question.Id && a.IsCorrect)
                                .Count();

                            if (question.TypeAnswer == 2)
                            {
                                totalPlayerAccuracy += (double)correctAnswersForQuestion / totalCorrectAnswersForQuestion;
                            }
                            else
                            {
                                totalPlayerAccuracy += correctAnswersForQuestion;
                            }
                        }

                        var playerAccuracy = totalPlayerAccuracy / questionCount;

                        if (playerAccuracy < 0.5)
                        {
                            reportDetail.PlayersBelowAvg.Add(new ReportBelowAvg
                            {
                                PlayerId = playerSession.Id,
                                PlayerName = playerSession.Nickname,
                                Accuracy = (int)Math.Round(playerAccuracy * 100)
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
                if (!IsBase64String(slug))
                {
                    // Xử lý lỗi: Slug không hợp lệ
                    return RedirectToAction("Error404","Home");
                }
                var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
                var parts = decodedSlug.Split('-');
                if (parts.Length == 0 || !int.TryParse(parts[0], out var id))
                {
                    // Xử lý lỗi: Slug không hợp lệ
                    return RedirectToAction("Error404", "Home");
                }
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
                while (getResultPodium.Count < 3)
                {
                    getResultPodium.Add(new ReportPodium { Nickname = "", TotalScore = 0, Accessory = "", AvatarUrl = "" });
                }
                return View(getResultPodium);
            }
            return View();
        }
        [Route("reports/detail/player/all/{slug}")]
        public async Task<IActionResult> ReportPlayer(string slug)
        {
            if (!IsBase64String(slug))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
            var parts = decodedSlug.Split('-');
            if (parts.Length == 0 || !int.TryParse(parts[0], out var id))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
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
                           .Where(ps => ps.QuizSessionId == q.Id).AsNoTracking()
                           .ToList(),
                       Questions = _context.Questions
                           .Where(qst => qst.EduQuizId == q.EduQuizId)
                           .Include(qst => qst.Choices).AsNoTracking()
                           .ToList()
                   })
                   .FirstOrDefaultAsync();

                if (quizSessionData != null)
                {
                    var playerCount = quizSessionData.PlayerSessions.Count;
                    var questionCount = quizSessionData.Questions.Count;

                    var players = quizSessionData.PlayerSessions
                    .OrderByDescending(p => p.TotalScore)
                    .Select(p => new
                    {
                        NickName = p.Nickname,
                        TotalScore = p.TotalScore,
                        Id = p.Id
                    })
                    .ToList();
                    if (players != null)
                    {
                        var getquizsession = await _context.QuizSessions
                             .Where(n => n.Id == id)
                             .Select(n => n.EduQuizId)
                             .FirstOrDefaultAsync();
                        var getquestions = await _context.Questions
                            .Where(q => q.EduQuizId == getquizsession)
                            .Include(q => q.Choices)
                            .Select(q => new
                            {
                                q.Id,
                                q.QuestionText,
                                q.TypeQuestion,
                                q.TypeAnswer,
                                Order = _context.QuizSessionQuestions
                                .Where(o => o.QuizSessionId == id && o.QuestionId == q.Id)
                                .Select(o => o.Order)
                                .FirstOrDefault(),
                                q.Choices
                            }).OrderBy(q => q.Order)
                            .ToListAsync();

                        var reportPlayers = new List<ReportRankPlayer>();
                        var rank = 1;
                        foreach (var player in players)
                        {
                            double totalPlayerAccuracy = 0.0;
                            var countanswerd = 0; 
                            var playerAnswers = await _context.PlayerAnswers.Where(p => p.PlayerSessionId == player.Id && p.PlayerSession.QuizSessionId == id).ToListAsync();
                            foreach (var question in getquestions)
                            {
                                var playerAnswersForQuestion = playerAnswers
                                    .Where(pa => pa.QuestionId == question.Id)
                                    .ToList();
                                if (playerAnswersForQuestion.Any())
                                {
                                    countanswerd++;
                                    var correctAnswersForQuestion = playerAnswersForQuestion.Count(pa => pa.IsCorrect);
                                    var totalCorrectAnswersForQuestion = question.Choices
                                        .Where(a => a.IsCorrect)
                                        .Count();

                                    if (question.TypeAnswer == 2)
                                    {
                                        totalPlayerAccuracy += (double)correctAnswersForQuestion / totalCorrectAnswersForQuestion;
                                    }
                                    else
                                    {
                                        totalPlayerAccuracy += correctAnswersForQuestion;
                                    }

                                }
                            }
                            var playerAccuracy = totalPlayerAccuracy > 0 ? totalPlayerAccuracy / getquestions.Count * 100 : 0;
                            reportPlayers.Add(new ReportRankPlayer{
                                NickName = player.NickName,
                                TotalScore = player.TotalScore,
                                Rank = rank,
                                Id = player.Id,
                                Unanwered = questionCount - countanswerd,
                                ValueAccuracy = CalculateHelper.CalculateValueFromPercentageQuestion((int)Math.Round(playerAccuracy)),
                                Accuracy = Math.Round(playerAccuracy) + "%",
                            });
                            rank++;
                        }
                        
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
                            ReportPlayers = reportPlayers,
                        };
                        return View(reportDetail);
                    }
                }
            }
            return View();
        }
        [Route("reports/detail/player/belowavg/{slug}")]
        public async Task<IActionResult> ReportPlayerBelowAvg(string slug)
        {
            if (!IsBase64String(slug))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
            var parts = decodedSlug.Split('-');
            if (parts.Length == 0 || !int.TryParse(parts[0], out var id))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
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
                           .Where(ps => ps.QuizSessionId == q.Id).AsNoTracking()
                           .ToList(),
                       Questions = _context.Questions
                           .Where(qst => qst.EduQuizId == q.EduQuizId)
                           .Include(qst => qst.Choices).AsNoTracking()
                           .ToList()
                   })
                   .FirstOrDefaultAsync();

                if (quizSessionData != null)
                {
                    var playerCount = quizSessionData.PlayerSessions.Count;
                    var questionCount = quizSessionData.Questions.Count;

                    var players = quizSessionData.PlayerSessions
                    .OrderByDescending(p => p.TotalScore)
                    .Select(p => new
                    {
                        NickName = p.Nickname,
                        TotalScore = p.TotalScore,
                        Id = p.Id
                    })
                    .ToList();
                    if (players != null)
                    {
                        var getquizsession = await _context.QuizSessions
                             .Where(n => n.Id == id)
                             .Select(n => n.EduQuizId)
                             .FirstOrDefaultAsync();
                        var getquestions = await _context.Questions
                            .Where(q => q.EduQuizId == getquizsession)
                            .Include(q => q.Choices)
                            .Select(q => new
                            {
                                q.Id,
                                q.QuestionText,
                                q.TypeQuestion,
                                q.TypeAnswer,
                                Order = _context.QuizSessionQuestions
                                .Where(o => o.QuizSessionId == id && o.QuestionId == q.Id)
                                .Select(o => o.Order)
                                .FirstOrDefault(),
                                q.Choices
                            }).OrderBy(q => q.Order)
                            .ToListAsync();

                        var reportPlayers = new List<ReportRankPlayer>();
                        var rank = 1;
                        foreach (var player in players)
                        {
                            double totalPlayerAccuracy = 0.0;
                            var countanswerd = 0;
                            var playerAnswers = await _context.PlayerAnswers.Where(p => p.PlayerSessionId == player.Id && p.PlayerSession.QuizSessionId == id).ToListAsync();
                            foreach (var question in getquestions)
                            {
                                var playerAnswersForQuestion = playerAnswers
                                    .Where(pa => pa.QuestionId == question.Id)
                                    .ToList();
                                if (playerAnswersForQuestion.Any())
                                {
                                    countanswerd++;
                                    var correctAnswersForQuestion = playerAnswersForQuestion.Count(pa => pa.IsCorrect);
                                    var totalCorrectAnswersForQuestion = question.Choices
                                        .Where(a => a.IsCorrect)
                                        .Count();

                                    if (question.TypeAnswer == 2)
                                    {
                                        totalPlayerAccuracy += (double)correctAnswersForQuestion / totalCorrectAnswersForQuestion;
                                    }
                                    else
                                    {
                                        totalPlayerAccuracy += correctAnswersForQuestion;
                                    }

                                }
                            }
                            var playerAccuracy = totalPlayerAccuracy > 0 ? totalPlayerAccuracy / getquestions.Count * 100 : 0;
                            if((int)Math.Round(playerAccuracy) < 50)
                            {
                                reportPlayers.Add(new ReportRankPlayer
                                {
                                    NickName = player.NickName,
                                    TotalScore = player.TotalScore,
                                    Rank = rank,
                                    Id = player.Id,
                                    Unanwered = questionCount - countanswerd,
                                    ValueAccuracy = CalculateHelper.CalculateValueFromPercentageQuestion((int)Math.Round(playerAccuracy)),
                                    Accuracy = Math.Round(playerAccuracy) + "%",
                                });
                            }
                            rank++;
                        }

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
                            ReportPlayers = reportPlayers,
                        };
                        return View(reportDetail);
                    }
                }
            }
            return View();
        }
        [Route("reports/detail/player/unfinish/{slug}")]
        public async Task<IActionResult> ReportPlayerUnfinish(string slug)
        {
            if (!IsBase64String(slug))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
            var parts = decodedSlug.Split('-');
            if (parts.Length == 0 || !int.TryParse(parts[0], out var id))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
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
                           .Where(ps => ps.QuizSessionId == q.Id).AsNoTracking()
                           .ToList(),
                       Questions = _context.Questions
                           .Where(qst => qst.EduQuizId == q.EduQuizId)
                           .Include(qst => qst.Choices).AsNoTracking()
                           .ToList()
                   })
                   .FirstOrDefaultAsync();

                if (quizSessionData != null)
                {
                    var playerCount = quizSessionData.PlayerSessions.Count;
                    var questionCount = quizSessionData.Questions.Count;

                    var players = quizSessionData.PlayerSessions
                     .OrderByDescending(p => p.TotalScore)
                     .Select(p => new
                     {
                         NickName = p.Nickname,
                         TotalScore = p.TotalScore,
                         Id = p.Id
                     })
                     .ToList();
                    if (players != null)
                    {
                        var getquizsession = await _context.QuizSessions
                             .Where(n => n.Id == id)
                             .Select(n => n.EduQuizId)
                             .FirstOrDefaultAsync();
                        var getquestions = await _context.Questions
                            .Where(q => q.EduQuizId == getquizsession)
                            .Include(q => q.Choices)
                            .Select(q => new
                            {
                                q.Id,
                                q.QuestionText,
                                q.TypeQuestion,
                                q.TypeAnswer,
                                Order = _context.QuizSessionQuestions
                                .Where(o => o.QuizSessionId == id && o.QuestionId == q.Id)
                                .Select(o => o.Order)
                                .FirstOrDefault(),
                                q.Choices
                            }).OrderBy(q => q.Order)
                            .ToListAsync();

                        var reportPlayers = new List<ReportRankPlayer>();
                        var rank = 1;
                        foreach (var player in players)
                        {
                            double totalPlayerAccuracy = 0.0;
                            var countanswerd = 0;
                            var playerAnswers = await _context.PlayerAnswers.Where(p => p.PlayerSessionId == player.Id && p.PlayerSession.QuizSessionId == id).ToListAsync();
                            foreach (var question in getquestions)
                            {
                                var playerAnswersForQuestion = playerAnswers
                                    .Where(pa => pa.QuestionId == question.Id)
                                    .ToList();
                                if (playerAnswersForQuestion.Any())
                                {
                                    countanswerd++;
                                    var correctAnswersForQuestion = playerAnswersForQuestion.Count(pa => pa.IsCorrect);
                                    var totalCorrectAnswersForQuestion = question.Choices
                                        .Where(a => a.IsCorrect)
                                        .Count();

                                    if (question.TypeAnswer == 2)
                                    {
                                        totalPlayerAccuracy += (double)correctAnswersForQuestion / totalCorrectAnswersForQuestion;
                                    }
                                    else
                                    {
                                        totalPlayerAccuracy += correctAnswersForQuestion;
                                    }

                                }
                            }
                            var playerAccuracy = totalPlayerAccuracy > 0 ? totalPlayerAccuracy / getquestions.Count * 100 : 0;
                            if ((questionCount - countanswerd) != 0)
                            {
                                reportPlayers.Add(new ReportRankPlayer
                                {
                                    NickName = player.NickName,
                                    TotalScore = player.TotalScore,
                                    Rank = rank,
                                    Id = player.Id,
                                    Unanwered = questionCount - countanswerd,
                                    ValueAccuracy = CalculateHelper.CalculateValueFromPercentageQuestion((int)Math.Round(playerAccuracy)),
                                    Accuracy = Math.Round(playerAccuracy) + "%",
                                });
                            }
                            rank++;
                        }

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
                            ReportPlayers = reportPlayers,
                        };
                        return View(reportDetail);
                    }
                }
            }
            return View();
        }
        [Route("reports/detail/question/all/{slug}")]
        public async Task<IActionResult> ReportQuestion(string slug)
        {
            if (!IsBase64String(slug))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
            var parts = decodedSlug.Split('-');
            if (parts.Length == 0 || !int.TryParse(parts[0], out var id))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
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
                             .AsNoTracking()
                             .ToList(),
                         Questions = _context.Questions
                             .Where(qst => qst.EduQuizId == q.EduQuizId)
                             .Include(qst => qst.Choices)
                             .AsNoTracking()
                             .ToList(),
                         OrderQuestion = _context.EduQuizs
                             .Where(e=>e.Id == q.EduQuizId)
                             .Select(n => n.OrderQuestion)
                             .FirstOrDefault()
                     })
                     .FirstOrDefaultAsync();

                if (quizSessionData != null)
                {
                    var playerCount = quizSessionData.PlayerSessions.Count;
                    var questionCount = quizSessionData.Questions.Count;
                    List<int> orderQuestion = JsonConvert.DeserializeObject<List<int>>(quizSessionData.OrderQuestion);
                    var orderedQuestions = orderQuestion
                        .Select(id => quizSessionData.Questions.FirstOrDefault(q => q.Id == id))
                        .Where(q => q != null) 
                        .ToList();
                    var playerAnswers = await _context.PlayerAnswers
                        .Where(pa => pa.PlayerSession.QuizSessionId == quizSessionData.QuizSession.Id)
                        .AsNoTracking()
                        .ToListAsync();

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
                        ReportQuestions = new List<ReportRankQuestion>()
                    };

                    int count = 1;
                    foreach (var question in orderedQuestions)
                    {
                        var correctAnswersCount = playerAnswers.Count(pa => pa.QuestionId == question.Id && pa.IsCorrect);
                        var totalCorrectAnswersForQuestion = question.Choices
                            .Where(a => a.QuestionId == question.Id && a.IsCorrect)
                            .Count();
                        double accuracy;

                        if (question.TypeAnswer == 2)
                        {
                            accuracy = (double)correctAnswersCount / (playerCount * totalCorrectAnswersForQuestion);
                        }
                        else
                        {
                            accuracy = (double)correctAnswersCount / playerCount;
                        };
                        var questionAnswers = playerAnswers.Where(pa => pa.QuestionId == question.Id);
                        float averageTime = 0;
                        if (questionAnswers.Any())
                        {
                            averageTime = (float)Math.Round(questionAnswers.Average(pa => pa.TimeTaken), 2);
                        }

                        reportDetail.ReportQuestions.Add(new ReportRankQuestion
                        {
                            Id = question.Id,
                            TypeQuestion = question.TypeQuestion switch
                            {
                                "quiz" => "Câu đố",
                                "true_false" => "Đúng sai",
                                _ => "Nhập đáp án"
                            },
                            QuestionTitle = question.QuestionText,
                            Order = count,
                            Accuracy = (int)Math.Round(accuracy * 100) + "%",
                            ValueAccuracy = CalculateHelper.CalculateValueFromPercentageQuestion((int)Math.Round(accuracy * 100))
                        });
                        count++;
                    }
                    reportDetail.ReportQuestions = reportDetail.ReportQuestions.OrderBy(r => r.Order).ToList();
                    return View(reportDetail);
                }
            }
            return View();
        }
        [Route("reports/detail/question/difficult/{slug}")]
        public async Task<IActionResult> ReportQuestionDifficult(string slug)
        {
            if (!IsBase64String(slug))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
            var parts = decodedSlug.Split('-');
            if (parts.Length == 0 || !int.TryParse(parts[0], out var id))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
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
                             .Include(qst => qst.Choices)
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
                        ReportQuestions = new List<ReportRankQuestion>()
                    };


                    foreach (var question in quizSessionData.Questions)
                    {

                        var correctAnswersCount = playerAnswers.Count(pa => pa.QuestionId == question.Id && pa.IsCorrect);
                        var totalCorrectAnswersForQuestion = question.Choices
                            .Where(a => a.QuestionId == question.Id && a.IsCorrect)
                            .Count();
                        double accuracy;

                        if (question.TypeAnswer == 2)
                        {
                            accuracy = (double)correctAnswersCount / (playerCount * totalCorrectAnswersForQuestion);
                        }
                        else
                        {
                            accuracy = (double)correctAnswersCount / playerCount;
                        };
                        if (accuracy < 0.35)
                        {
                            var questionAnswers = playerAnswers.Where(pa => pa.QuestionId == question.Id);
                            float averageTime = 0;
                            if (questionAnswers.Any())
                            {
                                averageTime = (float)Math.Round(questionAnswers.Average(pa => pa.TimeTaken), 2);
                            }
                            var order = await _context.QuizSessionQuestions
                                .Where(o => o.QuizSessionId == quizSessionData.QuizSession.Id && o.QuestionId == question.Id)
                                .Select(o => o.Order)
                                .FirstOrDefaultAsync();
                            if(order != 0)
                            {
                                reportDetail.ReportQuestions.Add(new ReportRankQuestion
                                {
                                    Id = question.Id,
                                    TypeQuestion = question.TypeQuestion switch
                                    {
                                        "quiz" => "Câu đố",
                                        "true_false" => "Đúng sai",
                                        _ => "Nhập đáp án"
                                    },
                                    QuestionTitle = question.QuestionText,
                                    Order = order,
                                    Accuracy = (int)Math.Round(accuracy * 100) + "%",
                                    ValueAccuracy = CalculateHelper.CalculateValueFromPercentageQuestion((int)Math.Round(accuracy * 100))
                                });
                            }
                        }
                    }
                    reportDetail.ReportQuestions = reportDetail.ReportQuestions.OrderBy(r => r.Order).ToList();
                    return View(reportDetail);
                }
            }
            return View();
        }
        [Route("reports/detail/feedback/{slug}")]
        public async Task<IActionResult> ReportFeedback(string slug)
        {
            if (!IsBase64String(slug))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
            var parts = decodedSlug.Split('-');
            if (parts.Length == 0 || !int.TryParse(parts[0], out var id))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Error404", "Home");
            }
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
                             .AsNoTracking()
                             .ToList(),
                         Questions = _context.Questions
                             .Where(qst => qst.EduQuizId == q.EduQuizId)
                             .Include(qst => qst.Choices)
                             .AsNoTracking()
                             .ToList(),
                     })
                     .FirstOrDefaultAsync();
            var playerCount = quizSessionData.PlayerSessions.Count;
            var questionCount = quizSessionData.Questions.Count;
            var reportDetail = new ReportDetail
            {
                NameHost = quizSessionData.HostUsername,
                QuizSessionId = quizSessionData.QuizSession.Id,
                Type = quizSessionData.Type,
                StartTime = StringHelper.ConvertDateTimeToCustomString(quizSessionData.StartTime),
                Title = quizSessionData.Title,
                PlayerCount = playerCount,
                QuestionCount = questionCount,
            };
            var getfeedback = await _context.FeedbackQuizSessions.Where(f => f.QuizSessionId == id).ToListAsync();
            var datafeedback = new FeedbackViewModel
            {
                FlagFeedback = getfeedback.Count > 0,
                FlagRating = getfeedback.Where(f=>f.Rating == null).Count() != getfeedback.Count,
                Rating = getfeedback.Count > 0 ? getfeedback.Where(f => f.Rating.HasValue).Average(f => f.Rating.Value) : 0,
                FlagLearning = getfeedback.Where(f => f.PositiveLearningOutcome == null).Count() != getfeedback.Count,
                CountYesLearning = getfeedback.Where(f => f.PositiveLearningOutcome == true).Count(),
                CountNoLearning = getfeedback.Where(f => f.PositiveLearningOutcome == false).Count(),
                FlagLiked = getfeedback.Where(f => f.Liked == null).Count()!= getfeedback.Count,
                CountYesLiked = getfeedback.Where(f => f.Liked == true).Count(),
                CountNoLiked = getfeedback.Where(f => f.Liked == false).Count(),
                FlagFeeling = getfeedback.Where(f => f.PositiveFeeling == null).Count() != getfeedback.Count,
                CountYesFeeling = getfeedback.Where(f => f.PositiveFeeling == 1).Count(),
                CountNorFeeling = getfeedback.Where(f => f.PositiveFeeling == 2).Count(),
                CountNoFeeling = getfeedback.Where(f => f.PositiveFeeling == 0).Count(),
                ListFeedback = getfeedback
            };
            ViewBag.Feedback = datafeedback;
            return View(reportDetail);
        }
        [Route("reports/detail/print/{slug}")]
        public async Task<IActionResult> PrintReportSummary(string slug)
        {
            if (!IsBase64String(slug))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var decodedSlug = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(slug));
            var parts = decodedSlug.Split('-');
            if (parts.Length == 0 || !int.TryParse(parts[0], out var id))
            {
                // Xử lý lỗi: Slug không hợp lệ
                return RedirectToAction("Error404", "Home");
            }
            var authCookie = Request.Cookies["acToken"];
            if (authCookie == null)
            {
                return RedirectToAction("Error404", "Home");
            }
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
                    q.StartTime,
                    q.EndTime,
                    q.EduQuizId,
                    q.Title,
                    PlayerSessions = _context.PlayerSessions
                        .Where(ps => ps.QuizSessionId == q.Id).AsNoTracking()
                        .ToList(),
                    Questions = _context.Questions
                        .Where(qst => qst.EduQuizId == q.EduQuizId)
                        .Include(qst => qst.Choices).AsNoTracking()
                        .ToList(),
                    OrderQuestion = _context.EduQuizs
                        .Where(e => e.Id == q.EduQuizId)
                        .Select(n => n.OrderQuestion)
                        .FirstOrDefault(),
                    PlayerAnswer = _context.PlayerAnswers
                        .Where(pa => pa.PlayerSession.QuizSessionId == q.Id)
                        .AsNoTracking()
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (quizSessionData == null)
            {
                return View();
            }
            var playerCount = quizSessionData.PlayerSessions.Count;
            var questionCount = quizSessionData.Questions.Count;

            List<int> orderQuestion = JsonConvert.DeserializeObject<List<int>>(quizSessionData.OrderQuestion);
            var orderedQuestions = orderQuestion
                .Select(id => quizSessionData.Questions.FirstOrDefault(q => q.Id == id))
                .Where(q => q != null)
                .ToList();
            var players = quizSessionData.PlayerSessions
                .OrderByDescending(p => p.TotalScore)
                .Select(p => new
                {
                    NickName = p.Nickname,
                    TotalScore = p.TotalScore,
                    Id = p.Id
                })
                .ToList();
            // Tổng tỷ lệ chính xác cho tất cả các câu hỏi
            double totalAccuracySum = 0;

            var reportDetail = new ReportDetail
            {
                NameHost = quizSessionData.HostUsername,
                QuizSessionId = quizSessionData.QuizSession.Id,
                Type = quizSessionData.Type,
                Title = quizSessionData.Title,
                PlayerCount = playerCount,
                QuestionCount = questionCount,
                ReportPlayers = new List<ReportRankPlayer>(),
                ReportQuestions = new List<ReportRankQuestion>()
            };

            int count = 1, countquestiondifficult = 0;
            foreach (var question in orderedQuestions)
            {
                var correctAnswersCount = quizSessionData.PlayerAnswer.Count(pa => pa.QuestionId == question.Id && pa.IsCorrect);
                var totalCorrectAnswersForQuestion = question.Choices
                    .Where(a => a.QuestionId == question.Id && a.IsCorrect)
                    .Count();
                var order = await _context.QuizSessionQuestions
                        .Where(o => o.QuizSessionId == quizSessionData.QuizSession.Id && o.QuestionId == question.Id)
                        .AsNoTracking()
                        .Select(o => o.Order)
                        .FirstOrDefaultAsync();

                var accuracy = question.TypeAnswer == 2
                    ? (double)correctAnswersCount / (playerCount * totalCorrectAnswersForQuestion)
                    : (double)correctAnswersCount / playerCount;

                totalAccuracySum += accuracy;

                if (order != 0 && accuracy < 0.35)
                {
                    countquestiondifficult++;
                }

                reportDetail.ReportQuestions.Add(new ReportRankQuestion
                {
                    Id = question.Id,
                    TypeQuestion = question.TypeQuestion switch
                    {
                        "quiz" => "Câu đố",
                        "true_false" => "Đúng sai",
                        _ => "Nhập đáp án"
                    },
                    QuestionTitle = question.QuestionText,
                    Order = count,
                    Accuracy = (int)Math.Round(accuracy * 100) + "%",
                });
                count++;
            }
            reportDetail.TotalAccuracy = questionCount > 0
               ? (int?)Math.Round(totalAccuracySum / questionCount * 100)
               : null;
            var reportPlayers = new List<ReportRankPlayer>();
            var rank = 1;
            var countUnfinish = 0;
            var countBelowAvg = 0;
            foreach (var player in players)
            {
                double totalPlayerAccuracy = 0.0;
                var countanswerd = 0;
                var playerAnswers = quizSessionData.PlayerAnswer.Where(p => p.PlayerSessionId == player.Id).ToList();
                foreach (var question in orderedQuestions)
                {
                    var playerAnswersForQuestion = playerAnswers
                        .Where(pa => pa.QuestionId == question.Id)
                        .ToList();
                    if (playerAnswersForQuestion.Any())
                    {
                        countanswerd++;
                        var correctAnswersForQuestion = playerAnswersForQuestion.Count(pa => pa.IsCorrect);
                        var totalCorrectAnswersForQuestion = question.Choices
                            .Where(a => a.IsCorrect)
                            .Count();

                        if (question.TypeAnswer == 2)
                        {
                            totalPlayerAccuracy += (double)correctAnswersForQuestion / totalCorrectAnswersForQuestion;
                        }
                        else
                        {
                            totalPlayerAccuracy += correctAnswersForQuestion;
                        }
                    }
                }
                if (countanswerd != orderedQuestions.Count)
                {
                    countUnfinish++;
                }
                var playerAccuracy = totalPlayerAccuracy > 0 ? totalPlayerAccuracy / orderedQuestions.Count * 100 : 0;
                if (playerAccuracy < 50)
                {
                    countBelowAvg++;
                }
                reportPlayers.Add(new ReportRankPlayer
                {
                    NickName = player.NickName,
                    TotalScore = player.TotalScore,
                    Rank = rank,
                    Id = player.Id,
                    Unanwered = questionCount - countanswerd,
                    Accuracy = Math.Round(playerAccuracy) + "%",
                });
                rank++;
            }
            reportDetail.ReportPlayers = reportPlayers;
            reportDetail.TotalQuestionDifficults = countquestiondifficult.ToString();
            reportDetail.TotalBelowAvg = countBelowAvg.ToString();
            reportDetail.TotalUnfinish = countUnfinish.ToString() + "/" + playerCount;
            return View(reportDetail);
        }
        #region handle
        public async Task<IActionResult> ExportReport(int id)
        {
            // Đường dẫn tới file Excel mẫu
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/src/templates", "ReportTemplate.xlsx");

            // Mở file mẫu để đọc
            using (var package = new ExcelPackage(new FileInfo(templatePath)))
            {
               
                var reportData = await _context.QuizSessions
                    .Where(q => q.Id == id)
                    .Include(q => q.HostUser)
                    .Select(q => new
                    {
                        QuizSession = q,
                        HostUsername = q.HostUser.Username,
                        StartTime = q.StartTime ?? DateTime.Now,
                        Title = q.Title,
                        PlayerSessions = _context.PlayerSessions
                            .Where(ps => ps.QuizSessionId == q.Id)
                            .OrderByDescending(pa => pa.TotalScore)
                            .AsNoTracking()
                            .ToList(),
                        Questions = _context.Questions
                            .Where(qst => qst.EduQuizId == q.EduQuizId)
                            .Include(qst => qst.Choices).AsNoTracking()
                            .ToList(),
                        OrderQuizSessionQuestion = _context.QuizSessionQuestions
                            .Where(o=>o.QuizSessionId == id)
                            .AsNoTracking()
                            .ToList(),
                        Feedback = _context.FeedbackQuizSessions
                            .Where(f => f.QuizSessionId == id)
                            .AsNoTracking()
                            .ToList(),
                        PlayerAnswers = _context.PlayerAnswers
                            .Where(pa => pa.PlayerSession.QuizSessionId == q.Id)
                            .AsNoTracking().ToList()
                    })
                    .FirstOrDefaultAsync();
                var playerCount = reportData.PlayerSessions.Count;
                var questionCount = reportData.Questions.Count;

            
                var reportDetail = new ReportDetail
                {
                    NameHost = reportData.HostUsername,
                    StartTime = StringHelper.ConvertDateTimeToCustomString(reportData.StartTime),
                    Title = reportData.Title,
                    PlayerCount = playerCount,
                    QuestionCount = questionCount,
                };
                // Tổng tỷ lệ chính xác cho tất cả các câu hỏi
                double totalAccuracySum = 0;

                var questionAccuracy = new Dictionary<int, double>();
                foreach (var question in reportData.Questions)
                {
                    var correctAnswersCount = reportData.PlayerAnswers.Count(pa => pa.QuestionId == question.Id && pa.IsCorrect);
                    var totalCorrectAnswersForQuestion = question.Choices.Count(a => a.QuestionId == question.Id && a.IsCorrect);

                    double accuracy;
                    if (question.TypeAnswer == 2)
                    {
                        accuracy = (double)correctAnswersCount / (playerCount * totalCorrectAnswersForQuestion);
                    }
                    else
                    {
                        accuracy = (double)correctAnswersCount / playerCount;
                    }

                    questionAccuracy[question.Id] = accuracy;
                    totalAccuracySum += accuracy;
                }
                var listquestion = new List<ReportQuestionExport>();
                var listplayer = new List<ReportQuestionByPlayer>();
                int rank = 1;
                var addedQuestions = new HashSet<string>();

                foreach (var player in reportData.PlayerSessions)
                {
                    var resultplayer = new ReportQuestionByPlayer
                    {
                        Nickname = player.Nickname,
                        Rank = rank,
                        TotalScore = player.TotalScore,
                        AnswerQuestions = new List<AnswerQuestionByPlayer>()
                    };

                    var playerAnswers = reportData.PlayerAnswers.Where(p => p.PlayerSessionId == player.Id).ToList();

                    foreach (var question in reportData.Questions)
                    {
                        var getquestion = reportData.OrderQuizSessionQuestion.FirstOrDefault(q => q.QuestionId == question.Id);

                        if (getquestion != null)
                        {
                            if (!addedQuestions.Contains(question.Id.ToString()))
                            {
                                var questionExport = new ReportQuestionExport
                                {
                                    Order = getquestion.Order,
                                    QuestionTitle = question.QuestionText,
                                    QuestionId = question.Id,
                                    TypeQuestion = question.TypeQuestion switch
                                    {
                                        "quiz" => "Câu đố",
                                        "true_false" => "Đúng hoặc sai",
                                        _ => "Nhập đáp án"
                                    },
                                    Accuracy = questionAccuracy[question.Id],
                                    TimeQuestion = question.Time ?? 0,
                                    ListChoice = new List<ChoiceExport>()
                                };

                                foreach (var choice in question.Choices)
                                {
                                    var answerChoice = reportData.PlayerAnswers.Where(c => c.QuestionId == question.Id && c.ChoiceId == choice.Id).ToList();
                                    double avgtimeChoice = answerChoice.Count > 0 ? answerChoice.Average(ac => ac.TimeTaken) : 0;
                                    var choiceExport = new ChoiceExport
                                    {
                                        Answer = choice.Answer,
                                        IsCorrect = choice.IsCorrect,
                                        DisplayOrder = choice.DisplayOrder,
                                        CountAnswer = answerChoice.Count,
                                        TimeAvg = avgtimeChoice,
                                    };
                                    questionExport.ListChoice.Add(choiceExport);
                                }

                                // Add the question to the list
                                listquestion.Add(questionExport);
                                addedQuestions.Add(question.Id.ToString()); // Mark this question as added
                            }

                            // Process answers for the current question
                            var playerAnswersForQuestion = playerAnswers
                               .Where(pa => pa.QuestionId == question.Id)
                               .ToList();

                            if (playerAnswersForQuestion.Any())
                            {
                                var point = 0;
                                double timetaken = 0.00;
                                var answers = new List<string>();

                                foreach (var answer in playerAnswersForQuestion)
                                {
                                    var answerToAdd = answer.ChoiceId != null
                                         ? question.Choices.FirstOrDefault(c => c.Id == answer.ChoiceId)?.Answer
                                         : answer.AnswerText;

                                    answers.Add(answerToAdd);
                                    point += answer.Points;
                                    timetaken = answer.TimeTaken;
                                }

                                resultplayer.AnswerQuestions.Add(new AnswerQuestionByPlayer
                                {
                                    QuestionId = question.Id,
                                    AnswerText = string.Join("    |+|    ", answers),
                                    Point = point,
                                    Iscorrect = point > 0,
                                    TimeTaken = timetaken,
                                    Order = getquestion.Order
                                });
                            }
                            else
                            {
                                resultplayer.AnswerQuestions.Add(new AnswerQuestionByPlayer
                                {
                                    QuestionId = question.Id,
                                    AnswerText = "Không có đáp án",
                                    Point = 0,
                                    Iscorrect = false,
                                    TimeTaken = 0,
                                    Order = getquestion.Order
                                });
                            }
                        }
                    }

      
                    resultplayer.CountCorrectAnswer = resultplayer.AnswerQuestions.Count(i => i.Iscorrect);
                    resultplayer.CountWrongAnswer = resultplayer.AnswerQuestions.Count(i => !i.Iscorrect);
                    listplayer.Add(resultplayer);
                    rank++;
                }
                var totalAccuracy = questionCount > 0
                     ? Math.Round(totalAccuracySum / questionCount, 2)
                     : (double?)null;
                var totalInaccuracy = totalAccuracy.HasValue
                    ? Math.Round(1 - totalAccuracy.Value, 2)
                    : (double?)null;
                var avgScore = reportData.PlayerSessions.Sum(p => p.TotalScore) / playerCount;

                listquestion = listquestion.OrderBy(p => p.Order).ToList();
                foreach(var order in listplayer)
                {
                    order.AnswerQuestions = order.AnswerQuestions.OrderBy(p => p.Order).ToList();
                }    

                var worksheet = package.Workbook.Worksheets[0]; // Worksheet 1
                worksheet.Cells[1, 1].Value = reportData.Title;
                worksheet.Cells[2, 2].Value = reportDetail.StartTime;
                worksheet.Cells[3, 2].Value = reportDetail.NameHost;
                worksheet.Cells[4, 2].Value = $"{playerCount} Người chơi";
                worksheet.Cells[5, 2].Value = $"{reportData.OrderQuizSessionQuestion.Count} Trên {questionCount}";

                worksheet.Cells[8, 3].Value = totalAccuracy.HasValue ? totalAccuracy.Value : 0;
                worksheet.Cells[9, 3].Value = totalInaccuracy.HasValue ? totalInaccuracy.Value : 0;
                worksheet.Cells[10, 3].Value = avgScore;

                var feedbackCount = reportData.Feedback.Count;
                worksheet.Cells[13, 3].Value = $"{feedbackCount} lượt";
                var nonNullRatingsCount = reportData.Feedback.Count(f => f.Rating.HasValue);
                worksheet.Cells[14, 3].Value = nonNullRatingsCount > 0
                    ? reportData.Feedback.Where(f => f.Rating.HasValue).Average(f => f.Rating.Value)
                    : 0;

                var learningOutcomeCount = reportData.Feedback.Count(f => f.PositiveLearningOutcome != null);
                var positiveLearningOutcomeCount = reportData.Feedback.Count(f => f.PositiveLearningOutcome == true);
                var negativeLearningOutcomeCount = reportData.Feedback.Count(f => f.PositiveLearningOutcome == false);
                worksheet.Cells[15, 3].Value = learningOutcomeCount > 0 ? (double)positiveLearningOutcomeCount / learningOutcomeCount : 0;
                worksheet.Cells[15, 5].Value = learningOutcomeCount > 0 ? (double)negativeLearningOutcomeCount / learningOutcomeCount : 0;

                var likedCount = reportData.Feedback.Count(f => f.Liked != null);
                var likedPositiveCount = reportData.Feedback.Count(f => f.Liked == true);
                var likedNegativeCount = reportData.Feedback.Count(f => f.Liked == false);
                worksheet.Cells[16, 3].Value = likedCount > 0 ? (double)likedPositiveCount / likedCount : 0;
                worksheet.Cells[16, 5].Value = likedCount > 0 ? (double)likedNegativeCount / likedCount : 0;

                var feelingCount = reportData.Feedback.Count(f => f.PositiveFeeling != null);
                var positiveFeelingCount = reportData.Feedback.Count(f => f.PositiveFeeling == 1);
                var neutralFeelingCount = reportData.Feedback.Count(f => f.PositiveFeeling == 2);
                var negativeFeelingCount = reportData.Feedback.Count(f => f.PositiveFeeling == 0);
                worksheet.Cells[17, 4].Value = feelingCount > 0 ? (double)positiveFeelingCount / feelingCount : 0;
                worksheet.Cells[17, 6].Value = feelingCount > 0 ? (double)neutralFeelingCount / feedbackCount : 0;
                worksheet.Cells[17, 8].Value = feelingCount > 0 ? (double)negativeFeelingCount / feedbackCount : 0;

                
                var worksheet2 = package.Workbook.Worksheets[1]; // Worksheet 2

                worksheet2.Cells[1, 1].Value = reportData.Title;
                int startRowsheet2 = 4;
                double rowHeightsheet2 = worksheet2.Row(4).Height; 
                foreach (var player in listplayer)
                {
                    worksheet2.Cells[startRowsheet2, 1].Value = player.Rank;
                    worksheet2.Cells[startRowsheet2, 2].Value = player.Nickname;
                    worksheet2.Cells[startRowsheet2, 3].Value = player.TotalScore;
                    worksheet2.Cells[startRowsheet2, 4].Value = player.CountCorrectAnswer;
                    worksheet2.Cells[startRowsheet2, 5].Value = player.CountWrongAnswer;

                    worksheet2.Cells[startRowsheet2, 1, startRowsheet2, 5].StyleID = worksheet2.Cells[4, 1, 4, 5].StyleID;
                    worksheet2.Row(startRowsheet2).Height = rowHeightsheet2;
                    startRowsheet2++;
                }

                var worksheet3 = package.Workbook.Worksheets[2]; // Worksheet 3
                worksheet3.Cells[1, 1].Value = reportData.Title;
                var startRowsheet3 = 4;
                var startColsheet3 = 4;
                double rowHeightsheet3 = worksheet3.Row(4).Height;
                var count = 0;
                foreach (var player in listplayer)
                {
                    worksheet3.Cells[startRowsheet3, 1].Value = player.Rank;
                    worksheet3.Cells[startRowsheet3, 2].Value = player.Nickname;
                    worksheet3.Cells[startRowsheet3, 3].Value = player.TotalScore;

                    worksheet3.Cells[startRowsheet3, 1, startRowsheet3, 5].StyleID = worksheet3.Cells[4, 1, 4, 5].StyleID;
                    worksheet3.Row(startRowsheet3).Height = rowHeightsheet3;
                    var currentCol = startColsheet3;
                    foreach (var questionanswer in player.AnswerQuestions)
                    {

                        if (count < listquestion.Count)
                        {
                            worksheet3.Cells[3, currentCol].Value = $"Câu hỏi {listquestion[count].Order}";
                            worksheet3.Cells[3, currentCol + 1].Value = listquestion[count].QuestionTitle;

                            worksheet3.Cells[1, currentCol].StyleID = worksheet3.Cells[1, 4].StyleID;
                            worksheet3.Cells[2, currentCol].StyleID = worksheet3.Cells[2, 4].StyleID;
                            worksheet3.Cells[1, currentCol + 1].StyleID = worksheet3.Cells[1, 5].StyleID;
                            worksheet3.Cells[2, currentCol + 1].StyleID = worksheet3.Cells[2, 5].StyleID;

                            worksheet3.Cells[3, currentCol].StyleID = worksheet3.Cells[3, currentCol - 2].StyleID;
                            worksheet3.Cells[3, currentCol + 1].StyleID = worksheet3.Cells[3, currentCol - 1].StyleID;
                        }

                        worksheet3.Cells[startRowsheet3, currentCol].Value = questionanswer.Point;

                        worksheet3.Column(currentCol).Width = 9;           
                        worksheet3.Column(currentCol + 1).Width = 36.09;
                        worksheet3.Cells[startRowsheet3, currentCol].StyleID = worksheet3.Cells[4, currentCol-2].StyleID;
                        worksheet3.Cells[startRowsheet3, currentCol + 1].StyleID = worksheet3.Cells[4, currentCol - 1].StyleID;

                        worksheet3.Cells[startRowsheet3, currentCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet3.Cells[startRowsheet3, currentCol].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                        worksheet3.Cells[startRowsheet3, currentCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        if (questionanswer.Iscorrect)
                        {
                            worksheet3.Cells[startRowsheet3, currentCol].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#00B050"));
                        }
                        else
                        {
                            worksheet3.Cells[startRowsheet3, currentCol].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FF012B"));
                        }
                        worksheet3.Cells[startRowsheet3, currentCol + 1].Value = questionanswer.AnswerText;
                        currentCol += 2;
                       
                        count++;
                    }
                    startRowsheet3++;
                }

                var templateWorksheet = package.Workbook.Worksheets[3]; // Worksheet 4 mẫu
                int countQuesion = 1;

                foreach (var question in listquestion)
                {
                    var startCol = 3;
                    var startRow = 15;

                    // Clone worksheet cho mỗi câu hỏi
                    var clonedWorksheet = package.Workbook.Worksheets.Add($"{question.Order} - {question.TypeQuestion}", templateWorksheet);

                    var choiceCorrect = question.ListChoice.Where(c => c.IsCorrect).ToList();
                    var answerCorrectTextss = new List<string>();
                    foreach (var answer in choiceCorrect)
                    {
                        answerCorrectTextss.Add(answer.Answer);
                    }
                    clonedWorksheet.Cells[1, 1].Value = reportData.Title;
                    clonedWorksheet.Cells[2, 1].Value = $"{question.Order} - {question.TypeQuestion}";
                    clonedWorksheet.Cells[2, 2].Value = $"{question.QuestionTitle}";
                    clonedWorksheet.Cells[3, 3].Value = string.Join("    |+|    ", answerCorrectTextss);
                    clonedWorksheet.Cells[4, 3].Value = question.Accuracy;
                    clonedWorksheet.Cells[5, 3].Value = $"{question.TimeQuestion} giây";

                    var currentCol = startCol;
                    foreach (var choice in question.ListChoice.OrderBy(c => c.DisplayOrder))
                    {
                        clonedWorksheet.Cells[8, currentCol + 1].Value = choice.Answer;
                        clonedWorksheet.Cells[9, currentCol].Value = choice.IsCorrect ? "✔︎" : "✘";
                        clonedWorksheet.Cells[9, currentCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        if (choice.IsCorrect)
                        {
                            clonedWorksheet.Cells[9, currentCol].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#00B050"));
                        }
                        else
                        {
                            clonedWorksheet.Cells[9, currentCol].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FF012B"));
                        }
                        clonedWorksheet.Cells[10, currentCol].Value = choice.CountAnswer;
                        clonedWorksheet.Cells[11, currentCol].Value = choice.TimeAvg;
                        currentCol += 2;
                    }

                    var currentRow = startRow;
                    double rowHeightsheetCurrent = clonedWorksheet.Row(14).Height;
                    foreach (var player in listplayer)
                    {
                        clonedWorksheet.Row(currentRow).Height = rowHeightsheetCurrent;
                        clonedWorksheet.Cells[currentRow, 1, currentRow, 8].StyleID = clonedWorksheet.Cells[15, 1, 15, 8].StyleID;
                        clonedWorksheet.Cells[currentRow, 1].Value = player.Nickname;
                        clonedWorksheet.Cells[currentRow, 1, currentRow, 2].Merge = true;

                        var getanswerbyquestion = player.AnswerQuestions.FirstOrDefault(a => a.QuestionId == question.QuestionId);
                        clonedWorksheet.Cells[currentRow, 3].Value = getanswerbyquestion.Iscorrect ? "✔︎" : "✘";
                        clonedWorksheet.Cells[currentRow, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        if (getanswerbyquestion.Iscorrect)
                        {
                            clonedWorksheet.Cells[currentRow, 3].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#00B050"));
                        }
                        else
                        {
                            clonedWorksheet.Cells[currentRow, 3].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#FF012B"));
                        }
                        clonedWorksheet.Cells[currentRow, 3].Style.Font.Color.SetColor(ColorTranslator.FromHtml("#fff"));
                        clonedWorksheet.Cells[currentRow, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        clonedWorksheet.Cells[currentRow, 4].Value = getanswerbyquestion.AnswerText;
                        clonedWorksheet.Cells[currentRow, 5].Value = getanswerbyquestion.Point;
                        clonedWorksheet.Cells[currentRow, 5, currentRow, 6].Merge = true;
                        clonedWorksheet.Cells[currentRow, 7].Value = getanswerbyquestion.TimeTaken;
                        clonedWorksheet.Cells[currentRow, 7, currentRow, 8].Merge = true;

                        currentRow++;
                    }
                    countQuesion++;
                }
                package.Workbook.Worksheets.Delete(templateWorksheet);
                // Trả file Excel về client
                var excelBytes = package.GetAsByteArray();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
            }
        }
        public static bool IsBase64String(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return false;

            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
        public async Task<IActionResult> SaveNameReport(int id,string name)
        {
            var getReport = await _context.QuizSessions.FindAsync(id);
            if (getReport == null)
            {
                return Json(new { status = false });
            }
            getReport.Title = name;
            var result = await _context.SaveChangesAsync();
            return Json(new { status = result });
        }
        public async Task<IActionResult> GetReportbyQuestion(int idquestion, int quizsessionId)
        {
            var getQuestion = await _context.Questions
                .Where(q => q.Id == idquestion)
                .Select(n => new
                {
                    n.QuestionText,
                    n.Id,
                    n.TypeQuestion,
                    n.Image,
                    n.Time,
                    n.TypeAnswer,
                    Order = _context.QuizSessionQuestions
                        .Where(o => o.QuizSessionId == quizsessionId && o.QuestionId == n.Id)
                        .Select(o => o.Order)
                        .FirstOrDefault(),
                    Choices = n.Choices.Select(c => new { c.Id, c.Answer,c.IsCorrect }).ToList()
                }).FirstOrDefaultAsync();

            if (getQuestion == null)
            {
                return Json(new { status = false });
            }
            var playerAnswersRaw = await _context.PlayerAnswers
               .Where(pa => pa.PlayerSession.QuizSessionId == quizsessionId && pa.QuestionId == idquestion)
               .Include(pa => pa.PlayerSession) 
               .ToListAsync();


            var playerAnswers = playerAnswersRaw.Select(pa => new PlayerAnswerReport
            {
                PlayerId = pa.PlayerSessionId,
                Nickname = pa.PlayerSession.Nickname,
                Answered = pa.ChoiceId != null
                    ? getQuestion.Choices.FirstOrDefault(c => c.Id == pa.ChoiceId)?.Answer 
                    : pa.AnswerText,
                IsCorrect = pa.IsCorrect,
                TimeTaken = pa.TimeTaken.ToString(),
                Points = pa.Points
            }).ToList();

            var answeredPlayersId = playerAnswers.Select(p => p.PlayerId).ToList();

            var allPlayers = await _context.PlayerSessions
                .Where(ps => ps.QuizSessionId == quizsessionId)
                .Select(ps => new { ps.Id, ps.Nickname })
                .ToListAsync();

            var playersWithoutAnswers = allPlayers
                .Where(p => !answeredPlayersId.Contains(p.Id))
                .Select(p => new PlayerAnswerReport
                {
                    PlayerId = p.Id,
                    Nickname = p.Nickname,
                    Answered = "Không có câu trả lời",
                    IsCorrect = false,
                    TimeTaken = "--",
                    Points = 0
                }).ToList();

            // Gộp người chơi đã trả lời và không trả lời
            var allPlayerAnswers = playerAnswers.Concat(playersWithoutAnswers).ToList();

            // Tính toán thống kê cho các lựa chọn (Choices)
            var choiceStats = getQuestion.Choices.Select(c => new
            {
                Answer = c.Answer,
                Count = playerAnswers.Count(pa => pa.Answered == c.Answer),
                IsCorrect = c.IsCorrect,
                Percent = Math.Round((double)playerAnswers.Count(pa => pa.Answered == c.Answer) / (getQuestion.TypeAnswer == 2? playerAnswers.Count: allPlayers.Count) * 100, 2)
            }).ToList();

            // Thêm "Không có đáp án" vào thống kê
            choiceStats.Add(new
            {
                Answer = "Không có đáp án",
                Count = playersWithoutAnswers.Count,
                IsCorrect = false,
                Percent = Math.Round((double)playersWithoutAnswers.Count / allPlayers.Count * 100, 2)
            });
            // Tính toán số lượng người chơi đã trả lời đúng
            int playerAnsweredCount = playerAnswers.Select(pa => pa.PlayerId).Distinct().Count(); // Số người chơi đã trả lời
            int totalPlayers = allPlayers.Count;
            // Tính tỷ lệ chính xác cho các câu hỏi có TypeAnswer = 2
            double correctAccuracy;
            if (getQuestion.TypeAnswer == 2)
            {
                // Tính tổng số lựa chọn đúng
                var totalCorrectChoices = getQuestion.Choices.Count(c => c.IsCorrect);
                var totalAnswersGiven = playerAnswers.Count(pa => pa.IsCorrect); // Số câu trả lời đúng
                correctAccuracy = (double)totalAnswersGiven / (totalPlayers * totalCorrectChoices) * 100; // Chia theo số lượng người chơi đã trả lời
            }
            else
            {
                // Với câu hỏi bình thường
                correctAccuracy = (double)playerAnswers.Count(p => p.IsCorrect) / totalPlayers * 100; // Chia theo số lượng người chơi đã trả lời
            }
            var avgTime = playerAnswers.Count > 0 ? playerAnswers.Average(a => Convert.ToDouble(a.TimeTaken)) : 0;

            // Tạo phản hồi dữ liệu
            var dataResponse = new
            {
                getQuestion.Id,
                TypeQuestion = getQuestion.TypeQuestion switch
                {
                    "quiz" => "Câu đố",
                    "true_false" => "Đúng hoặc sai",
                    _ => "Nhập đáp án"
                },
                getQuestion.QuestionText,
                getQuestion.Image,
                getQuestion.Time,
                TypeAnswer = getQuestion.TypeAnswer == 1 ? "Chọn một" : "Chọn nhiều",
                getQuestion.Order,
                CorrectAccuracy = Math.Round(correctAccuracy),
                ValueAccuracy = CalculateHelper.CalculateValueFromPercentageQuestion((int)Math.Round(correctAccuracy)),
                AvgTime = Math.Round(avgTime, 2),
                PlayerAnsweredCount = $"{playerAnsweredCount} trên {totalPlayers}",
                Choices = choiceStats,
                PlayerAnswers = allPlayerAnswers
            };

            return Json(new { status =true, data= dataResponse });
        }
        public async Task<IActionResult> GetReportbyPlayer(int idplayer, int quizsessionId)
        {
            var players = await _context.PlayerSessions.Where(p =>p.QuizSessionId == quizsessionId)
            .OrderByDescending(p => p.TotalScore)
            .Select(p => new
            {
                NickName = p.Nickname,
                TotalScore = p.TotalScore,
                Id = p.Id
            })
            .ToListAsync();
            var rankedPlayer = players.Select((p, index) => new
            {
                NickName = p.NickName,
                TotalScore = p.TotalScore,
                Rank = index + 1, 
                Id = p.Id
            }).FirstOrDefault(p => p.Id == idplayer);
            if (rankedPlayer != null)
            {
                var getquizsession = await _context.QuizSessions
                     .Where(n => n.Id == quizsessionId)
                     .Select(n => n.EduQuizId)
                     .FirstOrDefaultAsync();
                var getquestions = await _context.Questions
                    .Where(q => q.EduQuizId == getquizsession)
                    .Include(q=>q.Choices)
                    .Select(q => new
                    {
                        q.Id,
                        q.QuestionText,
                        q.TypeQuestion,
                        q.TypeAnswer,
                        Order = _context.QuizSessionQuestions
                        .Where(o => o.QuizSessionId == quizsessionId && o.QuestionId == q.Id)
                        .Select(o => o.Order)
                        .FirstOrDefault(),
                        q.Choices
                    }).OrderBy(q => q.Order)
                    .ToListAsync();
                // Tạo list đáp án chi tiết
                var detailedAnswers = new List<object>();
                double totalPlayerAccuracy = 0.0;
                var countanswerd = 0;
                var playerAnswers = await _context.PlayerAnswers.Where(p => p.PlayerSessionId == idplayer).ToListAsync();
                foreach (var question in getquestions) 
                {
                    var playerAnswersForQuestion = playerAnswers
                        .Where(pa => pa.QuestionId == question.Id)
                        .ToList();
                    if (playerAnswersForQuestion.Any()) {
                        countanswerd++;
                        var correctAnswersForQuestion = playerAnswersForQuestion.Count(pa => pa.IsCorrect);
                        var totalCorrectAnswersForQuestion = question.Choices
                            .Where(a => a.IsCorrect)
                            .Count();

                        if (question.TypeAnswer == 2)
                        {
                            totalPlayerAccuracy += (double)correctAnswersForQuestion / totalCorrectAnswersForQuestion;
                        }
                        else
                        {
                            totalPlayerAccuracy += correctAnswersForQuestion;
                        }
                        foreach (var playeranswer in playerAnswersForQuestion)
                        {
                            detailedAnswers.Add(new
                            {
                                QuestionOrder = question.Order != 0 ? question.Order.ToString() : "Chưa chơi",
                                question.QuestionText,
                                TypeQuestion = question.TypeQuestion switch
                                {
                                    "quiz" => "Câu đố",
                                    "true_false" => "Đúng hoặc sai",
                                    _ => "Nhập đáp án"
                                },
                                Answer = playeranswer.ChoiceId != null
                                    ? question.Choices.FirstOrDefault(c => c.Id == playeranswer.ChoiceId)?.Answer
                                    : playeranswer.AnswerText,
                                IsCorrect = playeranswer.IsCorrect,
                                TimeTaken = playeranswer.TimeTaken,
                                Point = playeranswer.Points
                            });
                        }
                    }
                    else
                    {
                        detailedAnswers.Add(new
                        {
                            QuestionOrder = question.Order != 0 ? question.Order.ToString() : "Chưa chơi",
                            question.QuestionText,
                            TypeQuestion = question.TypeQuestion switch
                            {
                                "quiz" => "Câu đố",
                                "true_false" => "Đúng hoặc sai",
                                _ => "Nhập đáp án"
                            },
                            Answer = "Không có đáp án",
                            IsCorrect = false,
                            TimeTaken = "--",
                            Point = 0
                        });
                    }
                }
                var playerAccuracy = totalPlayerAccuracy > 0 ? totalPlayerAccuracy / getquestions.Count * 100: 0;
                var dataresponse = new
                {
                    rankedPlayer.NickName,
                    rankedPlayer.TotalScore,
                    Rank = rankedPlayer.Rank + " trên " + players.Count,
                    Answered = countanswerd + " trên " + getquestions.Count,
                    ValueAccuracy = CalculateHelper.CalculateValueFromPercentagePlayer((int)Math.Round(playerAccuracy)),
                    Accuracy = Math.Round(playerAccuracy) + "%",
                    DetailedAnswers = detailedAnswers
                }; 
                return Json(new { status = true,data= dataresponse });
            }
            return Json(new { status = false });
        }
        #endregion
    }
}
