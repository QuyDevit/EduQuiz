using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("PlayerQuizSessionQuestion")]
    public class PlayerQuizSessionQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? PlayerSessionId { get; set; }
        [ForeignKey("PlayerSessionId")]
        public virtual PlayerSession PlayerSession { get; set; }
        public string ListQuestionId { get; set; }
    }
}
