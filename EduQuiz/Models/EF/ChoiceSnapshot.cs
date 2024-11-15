using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("ChoiceSnapshot")]
    public class ChoiceSnapshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("QuestionId")]
        public int? QuestionId { get; set; }
        public virtual QuestionSnapshot Question { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
        public int DisplayOrder { get; set; }
    }
}
