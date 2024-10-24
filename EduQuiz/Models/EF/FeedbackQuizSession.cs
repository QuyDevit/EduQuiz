using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("FeedbackQuizSession")]
    public class FeedbackQuizSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? QuizSessionId { get; set; }
        [ForeignKey("QuizSessionId")]
        public virtual QuizSession QuizSession { get; set; }
        [Range(1, 5)]
        public int? Rating { get; set; }
        public bool? PositiveLearningOutcome { get; set; }
        public bool? Liked { get; set; }
        public int? PositiveFeeling { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
