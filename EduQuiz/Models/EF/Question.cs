using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("Question")]
    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("EduQuizId")]
        public int? EduQuizId { get; set; }
        public virtual EduQuiz EduQuiz { get; set; }
        public string? QuestionText { get; set; }
        public string? TypeQuestion { get; set; }
        public int? TypeAnswer { get; set; }
        public int? Time { get; set; }
        public int? PointsMultiplier { get; set; }
        public string? Image { get; set; }
        public ICollection<Choice> Choices { get; set; } = new List<Choice>();
        public string? ImageEffect { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool Status { get; set; } = true;
    }
}
