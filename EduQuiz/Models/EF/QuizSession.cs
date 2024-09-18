using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduQuiz.Models.EF
{
    [Table("QuizSession")]
    public class QuizSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("EduQuizId")]
        public int EduQuizId { get; set; }
        public virtual EduQuiz EduQuiz { get; set; }

        [ForeignKey("HostUserId")]
        public int HostUserId { get; set; }
        public virtual User HostUser { get; set; }

        public string Pin { get; set; } // Unique PIN for players to join
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsActive { get; set; } = true;
    }

}
