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
        public string Pin { get; set; } 
        public string Title { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsWaitingRoom { get; set; } = true;// true: phiên đang trong sảnh chờ
        public bool IsRandomQuestion { get; set; } = false;
        public bool IsRandomAnswer { get; set; } = false;
        public bool IsAuto { get; set; } = false;
        public bool IsShowQuestionAndAnswer { get; set; } = false;
        public bool IsShowAvatar { get; set; } = true;
        public int TypeQuizSession { get; set; } = 0; //0:Game , 1:bài tập
    }

}
