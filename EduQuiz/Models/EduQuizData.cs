using EduQuiz.Models.EF;

namespace EduQuiz.Models
{
    public class EduQuizData
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
        public List<QuestionData> Questions { get; set; }
        public DateTime? UpdateAt { get; set; }
    }
    public class QuestionData
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
    public class ChoiceData
    {
        public int Id { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public int DisplayOrder { get; set; }
    }
}
