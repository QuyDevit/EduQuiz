using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduQuiz.Models.EF
{
    [Table("EduQuiz")]
    public class EduQuiz
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid Uuid { get; set; } = Guid.NewGuid(); 
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageCover { get; set; }
        public int? Type { get; set; } = 0; // 0: Bản nháp , 1 Bản chính
        public bool Visibility { get; set; } = true; // Hiển thị (Công khai hoặc riêng tư)
        [ForeignKey("UserId")]
        public int? UserId { get; set; }
        public virtual User User { get; set; }
        [ForeignKey("FolderId")]
        public int? FolderId { get; set; } // Thư mục
        public virtual Folder Folder { get; set; }
        [ForeignKey("ThemeId")]
        public int? ThemeId { get; set; } // Chủ đề
        public virtual Theme Theme { get; set; }
        [ForeignKey("MusicId")]
        public int? MusicId { get; set; } // Nhạc sảnh
        public virtual Music Music { get; set; }
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public string? OrderQuestion { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool Status { get; set; } = true;
    }
}
