using System.ComponentModel.DataAnnotations;

namespace FoodOrderWeb.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }  // Liên kết với User

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Liên kết
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<CartItem>? CartItems { get; set; }
    }
}