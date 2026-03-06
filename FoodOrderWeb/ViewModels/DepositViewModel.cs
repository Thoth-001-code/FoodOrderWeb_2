using System.ComponentModel.DataAnnotations;

namespace FoodOrderWeb.ViewModels
{
    public class DepositViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập số tiền cần nạp")]
        [Range(10000, 10000000, ErrorMessage = "Số tiền nạp phải từ 10,000đ đến 10,000,000đ")]
        [Display(Name = "Số tiền nạp")]
        public decimal Amount { get; set; }

        [StringLength(255)]
        [Display(Name = "Ghi chú")]
        public string? Description { get; set; }

        // Các mốc tiền nạp nhanh
        public List<decimal> QuickAmounts => new List<decimal>
        {
            50000, 100000, 200000, 500000, 1000000
        };
    }
}