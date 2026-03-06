using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderWeb.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CartId { get; set; }  // Khóa ngoại đến Cart

        [Required]
        public int FoodItemId { get; set; }  // Khóa ngoại đến FoodItem

        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1-100")]
        public int Quantity { get; set; }

        [Display(Name = "Ngày thêm")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Liên kết
        [ForeignKey("CartId")]
        public virtual Cart? Cart { get; set; }

        [ForeignKey("FoodItemId")]
        public virtual FoodItem? FoodItem { get; set; }
    }
}