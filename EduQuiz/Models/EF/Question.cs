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
        public int? TypeAnswer { get; set; } // 1 Chọn 1, 2 Chọn nhiều
        public int? Time { get; set; }
        public int? PointsMultiplier { get; set; } // 1 điểm chuẩn, 2 nhân đôi, 0 không điểm
        public string? Image { get; set; }
        public ICollection<Choice> Choices { get; set; } = new List<Choice>();
        public string? ImageEffect { get; set; } // 3: 3x3, 5:5x5, 8:8x8
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool Status { get; set; } = true;
    }
}
