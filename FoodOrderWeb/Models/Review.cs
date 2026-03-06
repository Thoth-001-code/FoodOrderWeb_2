using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderWeb.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int FoodItemId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá từ 1-5 sao")]
        [Display(Name = "Số sao")]
        public int Rating { get; set; }

        [StringLength(1000)]
        [Display(Name = "Nhận xét")]
        public string? Comment { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("FoodItemId")]
        public virtual FoodItem? FoodItem { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}