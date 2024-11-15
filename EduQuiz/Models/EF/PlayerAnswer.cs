using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduQuiz.Models.EF
{
    [Table("PlayerAnswer")]
    public class PlayerAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("PlayerSessionId")]
        public int PlayerSessionId { get; set; }
        public virtual PlayerSession PlayerSession { get; set; }

        [ForeignKey("QuestionId")]
        public int QuestionId { get; set; }
        public virtual QuestionSnapshot Question { get; set; }

        [ForeignKey("ChoiceId")]
        public int? ChoiceId { get; set; } 
        public virtual ChoiceSnapshot Choice { get; set; }

        public bool IsCorrect { get; set; } 
        public double TimeTaken { get; set; } 
        public int Points { get; set; } 
        public string? AnswerText { get; set; }
    }

}
