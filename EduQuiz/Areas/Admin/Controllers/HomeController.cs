using EduQuiz.Areas.Admin.Models;
using EduQuiz.DatabaseContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CustomAuthorize("Admin")]
    public class HomeController : Controller
    {
        private readonly EduQuizDBContext _context;
        public HomeController(EduQuizDBContext context) {
            _context = context;
        } 
        [Route("admin/dashboard")]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var revenueToday = await _context.Orders
                .Where(order => order.CreateAt.Date == today && order.Status == "Success")
                .SumAsync(order => order.TotalPrice);
            var countAccount = await _context.Users.CountAsync(x=>x.Id != 8);
            var countProfileCheck = await _context.Profile.CountAsync(x=>x.Status == false);
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var revenueThisMonth = await _context.Orders
                .Where(order => order.CreateAt >= startOfMonth && order.CreateAt <= endOfMonth)
                .SumAsync(order => order.TotalPrice);
            var quizSessionToday = await _context.QuizSessions
                 .Where(q => (q.StartTime.HasValue ? q.StartTime.Value.Date : DateTime.Now.Date) == today && q.IsActive)
                 .Include(q => q.HostUser)
                 .GroupJoin(
                     _context.PlayerSessions,
                     quiz => quiz.Id,
                     player => player.QuizSessionId,
                     (quiz, players) => new QuizSessionSummary
                     {
                         HostUser = quiz.HostUser.Username,
                         Avatar = quiz.HostUser.ProfilePicture,
                         Pin = quiz.Pin,
                         SumPlayer = players.Count(),
                         CreatedAt = quiz.StartTime ?? DateTime.Now,
                     })
                 .ToListAsync();
            var overviewOrder = await _context.Orders.Where(o=>o.CreateAt.Date == today).Take(6).ToListAsync();
            var view = new DashboardViewModel
            {
                CountAccount = countAccount,
                CountProfileCheck = countProfileCheck,
                OrderToday = overviewOrder,
                QuizSessionToday = quizSessionToday,
                RevenueThisMonth=revenueThisMonth,
                RevenueToday=revenueToday
            };
            return View(view);
        }
    }
}
