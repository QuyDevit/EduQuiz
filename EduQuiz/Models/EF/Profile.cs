using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("Profile")]
    public class Profile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid Uuid { get; set; } = Guid.NewGuid();
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public string? LinkZalo { get; set; }
        public string? LinkYoutube { get; set; }
        public string? LinkFacebook { get; set; }
        public string? LinkInstagram { get; set; }
        public string Image { get; set; }
        public string TitlePage { get; set; }
        public string DescriptionPage { get; set; }
        public string ListEduQuizTop { get; set; }
        public string InfoDonate { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public bool Status { get; set; } = false;
    }
}
