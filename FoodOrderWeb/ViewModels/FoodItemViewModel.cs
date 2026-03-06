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
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(1000, 10000000, ErrorMessage = "Giá phải từ 1,000đ đến 10,000,000đ")]
        [Display(Name = "Giá")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Giá không hợp lệ")]
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

        [Display(Name = "Danh mục")]
        public string? CategoryName { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; }

        [Display(Name = "Đã bán")]
        public int SoldCount { get; set; }

        [Display(Name = "Đánh giá")]
        public double Rating { get; set; }

        [Display(Name = "Số lượng đánh giá")]
        public int RatingCount { get; set; }
    }

    public class FoodItemListViewModel
    {
        public IEnumerable<FoodItemViewModel> FoodItems { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int? CategoryId { get; set; }
        public string? SearchString { get; set; }
        public string? SortOrder { get; set; }
    }

    public class FoodItemCreateViewModel
    {
        [Required(ErrorMessage = "Tên món ăn không được để trống")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên món phải từ 3-200 ký tự")]
        [Display(Name = "Tên món")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(1000, 10000000, ErrorMessage = "Giá phải từ 1,000đ đến 10,000,000đ")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Hình ảnh")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Còn hàng")]
        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
    }

    public class FoodItemEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên món ăn không được để trống")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Tên món phải từ 3-200 ký tự")]
        [Display(Name = "Tên món")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
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
        public bool IsAvailable { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
    }
}