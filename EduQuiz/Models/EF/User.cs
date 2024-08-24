using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [ForeignKey("RoleId")]
        public int? RoleId { get; set; }
        public virtual Role Role { get; set; }

        public string? LinkToken { get; set; }
        public string? VerificationCode { get; set; }
        public string? Organization { get; set; } // Tổ chức

        [ForeignKey("WorkplaceTypeId")]
        public int? WorkplaceTypeId { get; set; } // Nơi làm việc
        public virtual WorkplaceType WorkplaceType { get; set; }

        public DateTime? LastLoginAt { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Favorite { get; set; }

        public string PrivacySettings { get; set; } // Thiết lập sự riêng tư
        public string? LanguagePreference { get; set; } // Optional field for language preference
        public bool EmailVerified { get; set; } = false; // Optional field for email verification status
        public int ViolationCount { get; set; } = 0;
        public string? SubscriptionType { get; set; } // Loại đăng ký gói
        public DateTime? SubscriptionStartDate { get; set; } // Ngày bắt đầu đăng ký gói
        public DateTime? SubscriptionEndDate { get; set; } // Ngày kết thúc đăng ký gói

        public DateTime? DateOfBirth { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Status { get; set; } = true;

    }
}
