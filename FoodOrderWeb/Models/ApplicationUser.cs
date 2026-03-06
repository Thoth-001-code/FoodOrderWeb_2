using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [PersonalData]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [PersonalData]
        [Display(Name = "Ngày đăng ký")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // THÊM CÁC DÒNG NÀY - Navigation properties
        public virtual Cart? Cart { get; set; }
        public virtual Wallet? Wallet { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }
    }
}