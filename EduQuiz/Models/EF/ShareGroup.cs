using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("ShareGroup")]
    public class ShareGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int GroupId { get; set; }

        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public int EduQuizId { get; set; }
        [ForeignKey("EduQuizId")]
        public virtual EduQuiz EduQuiz { get; set; }
        public DateTime SharedDate { get; set; } = DateTime.Now;
    }
}
