using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("EduQuizFavorite")]
    public class EduQuizFavorite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public int? EduQuizId { get; set; }

        [ForeignKey("EduQuizId")]
        public virtual EduQuiz EduQuiz { get; set; }
    }
}
