using EduQuiz.Models.EF;

namespace EduQuiz.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public float RevenueToday { get; set; }
        public int CountAccount { get; set; }
        public int CountProfileCheck { get; set; }
        public float RevenueThisMonth { get; set; }
        public List<QuizSessionSummary> QuizSessionToday { get; set; }
        public List<Order> OrderToday { get; set; }

    }
    public class QuizSessionSummary
    {
        public string HostUser { get; set; }
        public string Avatar { get; set; }
        public string Pin { get; set; }
        public int SumPlayer { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
