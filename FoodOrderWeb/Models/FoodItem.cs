using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderWeb.Models
{
    public class FoodItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên món ăn không được để trống")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên món phải từ 3-200 ký tự")]
        [Display(Name = "Tên món")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(1000, 10000000, ErrorMessage = "Giá phải từ 1,000đ đến 10,000,000đ")]
        [Display(Name = "Giá")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Còn hàng")]
        public bool IsAvailable { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign key
        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        // THÊM CÁC DÒNG NÀY - Navigation properties cho CartItems và OrderItems
        public virtual ICollection<CartItem>? CartItems { get; set; }
        public virtual ICollection<OrderItem>? OrderItems { get; set; }

        public virtual ICollection<Review>? Reviews { get; set; }
    }
}