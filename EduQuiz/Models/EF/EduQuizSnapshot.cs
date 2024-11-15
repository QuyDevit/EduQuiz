using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("EduQuizSnapshot")]
    public class EduQuizSnapshot
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("EduQuizId")]
        public int? EduQuizId { get; set; }
        public virtual EduQuiz EduQuiz { get; set; }
        public Guid Uuid { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageCover { get; set; }
        public int? Type { get; set; } = 0; // 0: Bản nháp , 1 Bản chính
        public bool Visibility { get; set; } = true; // Hiển thị (Công khai hoặc riêng tư) true: Công khai _=> Riêng tư
        [ForeignKey("UserId")]
        public int? UserId { get; set; }
        public virtual User User { get; set; }
        [ForeignKey("ThemeId")]
        public int? ThemeId { get; set; } // Chủ đề
        public virtual Theme Theme { get; set; }
        public int? TopicId { get; set; } // Thể loại
        [ForeignKey("TopicId")]
        public virtual Interest Topic { get; set; }
        [ForeignKey("MusicId")]
        public int? MusicId { get; set; } // Nhạc sảnh
        public virtual Music Music { get; set; }
        public virtual ICollection<QuestionSnapshot> Questions { get; set; } = new List<QuestionSnapshot>();
        public string? OrderQuestion { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public bool Status { get; set; } = true;
    }
}
