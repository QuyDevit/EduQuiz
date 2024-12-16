namespace EduQuiz.Models.API
{
    public class FeedbackRequest
    {
        public int? QuizSessionId { get; set; }
        public int? Rating { get; set; }
        public bool? PositiveLearningOutcome { get; set; }
        public bool? Liked { get; set; }
        public int? PositiveFeeling { get; set; }
    }
}
