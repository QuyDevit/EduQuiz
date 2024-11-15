using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduQuiz.Models.EF
{
    [Table("QuestionSnapshot")]
    public class QuestionSnapshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("EduQuizId")]
        public int? EduQuizId { get; set; }
        public virtual EduQuizSnapshot EduQuiz { get; set; }
        public string? QuestionText { get; set; }
        public string? TypeQuestion { get; set; }
        public int? TypeAnswer { get; set; } // 1 Chọn 1, 2 Chọn nhiều
        public int? Time { get; set; }
        public int? PointsMultiplier { get; set; } // 1 điểm chuẩn, 2 nhân đôi, 0 không điểm
        public string? Image { get; set; }
        public ICollection<ChoiceSnapshot> Choices { get; set; } = new List<ChoiceSnapshot>();
        public string? ImageEffect { get; set; } // 3: 3x3, 5:5x5, 8:8x8
    }
}
