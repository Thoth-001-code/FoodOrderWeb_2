using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FoodOrderWeb.ViewModels
{
    public class FoodItemViewModel
    {
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
        public decimal Price { get; set; }

        [Display(Name = "Hình ảnh")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Hình ảnh hiện tại")]
        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Còn hàng")]
        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
    }
}