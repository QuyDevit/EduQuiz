using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduQuiz.Models.EF
{
    [Table("Group")]
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid Uuid { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public string? Description { get; set; }
        public Guid? InviteCode { get; set; }
        public bool CanInviteNewMembers { get; set; } = true;
        public bool CanSeeMemberList { get; set; } = true;
        public bool CanShareContent { get; set; } = true;
        public bool CanPostContent { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public virtual ICollection<GroupMember> Members { get; set; }
        public bool Status { get; set; } = true;
    }
}
