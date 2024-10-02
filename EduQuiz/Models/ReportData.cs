namespace EduQuiz.Models
{
    public class ReportData
    {
        public int QuizSessionId { get; set; }
        public string Title { get; set; }
        public string Pin { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int PlayerCount { get; set; }
    }
    public class ReportPodium
    {
        public string? Accessory { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Nickname { get; set; }
        public int TotalScore { get; set; }
    }
    
    public class ReportDetail
    {
        public int QuizSessionId { get; set; }
        public string? Type { get; set; } // Game or Assignment
        public string? StartTime { get; set; }
        public string? Title { get; set; }
        public string? NameHost { get; set; }
        public int? PlayerCount { get; set; }
        public int? QuestionCount { get; set; }
        public double? TimeSession { get; set; }
        public int? TotalAccuracy { get; set; }
        public List<ReportQuestionDifficult> QuestionDifficults { get; set; }
        public List<ReportBelowAvg> PlayersBelowAvg { get; set; }
        public List<ReportNotFinish> PlayersNotFinish { get; set; }
    }
    public class ReportQuestionDifficult
    {
        public int QuestionId { get; set; }
        public string? TypeQuestion { get; set; }
        public string? QuestionTitle { get; set; }
        public string? QuestionImage { get; set; }
        public int? Order { get; set; }
        public int? Accuracy { get; set; }
        public float? AverageTime { get; set; }
    }
    public class ReportBelowAvg
    {
        public int PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public int? Accuracy { get; set; }
    }
    public class ReportNotFinish
    {
        public int PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public int? CountNotAnswer { get; set; }
    }
}
