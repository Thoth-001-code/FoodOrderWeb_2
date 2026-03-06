using System.ComponentModel.DataAnnotations;

namespace FoodOrderWeb.Models
{
    public class Category
    {
        [Key]  // Đánh dấu là khóa chính
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2-100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; }

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Liên kết đến bảng FoodItem (1 Category có nhiều FoodItem)
        public virtual ICollection<FoodItem>? FoodItems { get; set; }
    }
}