using System.ComponentModel.DataAnnotations;

namespace FoodOrderWeb.ViewModels
{
    public class DepositViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập số tiền cần nạp")]
        [Range(10000, 10000000, ErrorMessage = "Số tiền nạp phải từ 10,000đ đến 10,000,000đ")]
        [Display(Name = "Số tiền nạp")]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Số tiền không hợp lệ")]
        public decimal Amount { get; set; }

        [StringLength(255, ErrorMessage = "Ghi chú không được vượt quá 255 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Description { get; set; }

        [Display(Name = "Phương thức thanh toán")]
        public string? PaymentMethod { get; set; }

        // Danh sách số tiền nạp nhanh
        public List<decimal> QuickAmounts => new List<decimal>
        {
            50000, 100000, 200000, 500000, 1000000, 2000000, 5000000
        };
    }

    public class DepositResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public decimal NewBalance { get; set; }
        public DateTime TransactionTime { get; set; }
        public string TransactionId { get; set; }
    }
}