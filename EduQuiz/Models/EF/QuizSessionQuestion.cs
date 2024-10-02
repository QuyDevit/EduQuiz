using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("QuizSessionQuestion")]
    public class QuizSessionQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? QuizSessionId { get; set; }
        [ForeignKey("QuizSessionId")]
        public virtual QuizSession QuizSession { get; set; }
        public int? QuestionId { get; set; }
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
        public int Order { get; set; }
    }
}
