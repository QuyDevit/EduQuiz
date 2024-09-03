using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduQuiz.Models.EF
{
    public class Folder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid Uuid { get; set; } = Guid.NewGuid();

        [ForeignKey("UserId")]
        public int? UserId { get; set; }
        public virtual User User { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool Status { get; set; } = true;
        // Tham chiếu đến folder cha
        public int? ParentFolderId { get; set; }
        [ForeignKey("ParentFolderId")]
        public virtual Folder ParentFolder { get; set; }

        // Collection chứa các folder con
        public virtual ICollection<Folder> ChildFolders { get; set; } = new HashSet<Folder>();
        public virtual ICollection<QuizFolder> QuizFolders { get; set; } = new List<QuizFolder>();
    }
}
