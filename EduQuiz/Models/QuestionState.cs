namespace EduQuiz.Models
{
    public class QuestionState
    {
        public int CurrentQuestionIndex { get; set; }
        public int QuestionId { get; set; }
        public TimeSpan CountDowntime { get; set; }
        public int AnsweredCount { get; set; }
        public bool HasTimeUpBeenSent { get; set; }
    }
    public class PlayerData
    {
        public int Id { get; set; }
        public string Nickname { get; set; }
        public string AvatarUrl { get; set; }
        public string Accessory { get; set; }
    }
    public class QuizOption
    {
        public int QuizSessionId { get; set; }
        public bool IsRandomQuestion { get; set; }
        public bool IsRandomAnswer { get; set; }
        public bool IsShowAvatar { get; set; }
        public bool IsShowQAndA { get; set; }
        public bool IsAuto { get; set; }
        public string QuizTitle { get; set; }
        public int QuestionLength { get; set; }
    }
    public class EduQuizSession
    {
        public int Id { get; set; }
        public Guid Uuid { get; set; }
        public int? UserId { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string ImageCover { get; set; }
        public int? Type { get; set; }
        public bool Visibility { get; set; }
        public int? ThemeId { get; set; }
        public int? MusicId { get; set; }
        public List<QuestionSession> Questions { get; set; }
    }
    public class QuestionSession
    {
        public int Id { get; set; }
        public string TypeQuestion { get; set; }
        public int? TypeAnswer { get; set; }
        public string QuestionText { get; set; }
        public int? Time { get; set; }
        public int? PointsMultiplier { get; set; }
        public string Image { get; set; }
        public string ImageEffect { get; set; }
        public List<ChoiceData> Choices { get; set; }
    }
    public class ChoiceSession
    {
        public int Id { get; set; }
        public string Answer { get; set; }
        public int DisplayOrder { get; set; }
    }
}
