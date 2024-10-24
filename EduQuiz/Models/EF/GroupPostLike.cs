using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduQuiz.Models.EF
{
    [Table("GroupPostLike")]
    public class GroupPostLike
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int GroupPostId { get; set; }

        [ForeignKey("GroupPostId")]
        public virtual GroupPost GroupPost { get; set; }

        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public DateTime LikedDate { get; set; } = DateTime.Now;
    }
}
