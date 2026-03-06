using System.ComponentModel.DataAnnotations;

namespace FoodOrderWeb.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }
    }
}