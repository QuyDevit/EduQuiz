using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduQuiz.Models.EF
{
    [Table("GroupPost")]
    public class GroupPost
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
        public string? Content { get; set; }
        public string? Image { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.Now;
        public virtual ICollection<GroupPostLike> Likes { get; set; }
    }
}
