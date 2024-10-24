using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("AssignmentGroup")]
    public class AssignmentGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? GroupId { get; set; }

        [ForeignKey("GroupId")] 
        public virtual Group Group { get; set; }
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("QuizSessionId")]
        public int? QuizSessionId { get; set; }
        public virtual QuizSession QuizSession { get; set; }
        [ForeignKey("EduQuizId")]
        public int? EduQuizId { get; set; }
        public virtual EduQuiz EduQuiz { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.Now;
    }
}
