using EduQuiz.Models.EF;

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
        public string? TotalUnfinish { get; set; }
        public string? TotalBelowAvg { get; set; }
        public string? TotalQuestionDifficults { get; set; }
        public List<ReportQuestionDifficult> QuestionDifficults { get; set; }
        public List<ReportBelowAvg> PlayersBelowAvg { get; set; }
        public List<ReportNotFinish> PlayersNotFinish { get; set; }
        public List<ReportRankPlayer> ReportPlayers { get; set; }
        public List<ReportRankQuestion> ReportQuestions { get; set; }
    }
    public class FeedbackViewModel
    {
        public bool FlagFeedback { get; set; }
        public bool FlagRating { get; set; }
        public double Rating { get; set; }
        public bool FlagLearning { get; set; }
        public int CountYesLearning { get; set; }
        public int CountNoLearning { get; set; }
        public bool FlagLiked { get; set; }
        public int CountYesLiked { get; set; }
        public int CountNoLiked { get; set; }
        public bool FlagFeeling { get; set; }
        public int CountYesFeeling { get; set; }
        public int CountNorFeeling { get; set; }
        public int CountNoFeeling { get; set; }
        public List<FeedbackQuizSession> ListFeedback { get; set; }
    }
    public class ReportRankQuestion
    {
        public int Id { get; set; }
        public string? TypeQuestion { get; set; }
        public string? QuestionTitle { get; set; }
        public int? Order { get; set; }
        public string? Accuracy { get; set; }
        public double? ValueAccuracy { get; set; }
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
    public class ReportRankPlayer
    {
        public int Id { get; set; }
        public string? NickName { get; set; }
        public int? TotalScore { get; set; }
        public string? Accuracy { get; set; }
        public int? Rank { get; set; }
        public int? Unanwered { get; set; }
        public double? ValueAccuracy { get; set; }
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
    public class PlayerAnswerReport
    {
        public int PlayerId { get; set; }
        public string Nickname { get; set; }
        public string Answered { get; set; }
        public bool IsCorrect { get; set; }
        public string TimeTaken { get; set; }
        public int Points { get; set; }
    }

    public class ReportQuestionByPlayer
    {
        public int Rank { get; set; }
        public string Nickname { get; set; }
        public int TotalScore { get; set; }
        public int CountCorrectAnswer { get; set; }
        public int CountWrongAnswer { get; set; }
        public List<AnswerQuestionByPlayer> AnswerQuestions { get; set; }
    }
    public class AnswerQuestionByPlayer
    {
        public int QuestionId { get; set; }
        public int Order { get; set; }
        public int Point { get; set; }
        public string AnswerText { get; set; }
        public bool Iscorrect { get; set; }
        public double TimeTaken { get; set; } 
    }

    public class ReportQuestionExport
    {
        public int QuestionId { get; set; }
        public string QuestionTitle { get; set; }
        public string TypeQuestion { get; set; }
        public List<ChoiceExport> ListChoice { get; set; }
        public double Accuracy { get; set; }
        public int TimeQuestion { get; set; }
        public int Order { get; set; }
    }
    public class ChoiceExport {
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public int DisplayOrder { get; set; }
        public int CountAnswer { get; set; }
        public double TimeAvg { get; set; }
    }
}
