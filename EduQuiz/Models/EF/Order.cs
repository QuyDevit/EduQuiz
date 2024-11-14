using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz.Models.EF
{
    [Table("Order")]
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string OrderId { get; set; }
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public string  PlanType { get; set; } //Loại gói
        public int Quantity { get; set; }
        public string Period { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMethod { get; set; } // zalo, momo
        public float TotalPrice { get; set; }
        public DateTime CreateAt { get; set; }
        public string Status { get; set; } // Đang chờ, Thất bại, Thành công, Đã hủy
    }
}
