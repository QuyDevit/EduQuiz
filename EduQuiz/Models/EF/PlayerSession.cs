using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduQuiz.Models.EF
{
    [Table("PlayerSession")]
    public class PlayerSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("QuizSessionId")]
        public int QuizSessionId { get; set; }
        public virtual QuizSession QuizSession { get; set; }
        public string Nickname { get; set; }
        public string AvatarUrl { get; set; } 
        public string Accessory { get; set; }
        public int TotalScore { get; set; } = 0;
        public DateTime JoinedAt { get; set; } = DateTime.Now;
        public DateTime? FinishedAt { get; set; } 
        public int Rank { get; set; }
    }

}
