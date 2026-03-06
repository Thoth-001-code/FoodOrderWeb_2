using System.ComponentModel.DataAnnotations;

namespace FoodOrderWeb.ViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
    }
}