using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    public class QuizFolder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("EduQuiz")]
        public int EduQuizId { get; set; }
        public virtual EduQuiz EduQuiz { get; set; }

        [ForeignKey("Folder")]
        public int FolderId { get; set; }
        public virtual Folder Folder { get; set; }
    }
}
