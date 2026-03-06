using System.ComponentModel.DataAnnotations;
using FoodOrderWeb.Models;

namespace FoodOrderWeb.ViewModels
{
    public class CheckoutViewModel
    {
        public CheckoutViewModel()
        {
            CartItems = new List<CartItem>();
            PaymentMethod = PaymentMethod.Cash;
            ShippingFee = 0;
            Discount = 0;
        }

        // Danh sách món trong giỏ
        public List<CartItem> CartItems { get; set; }

        // Thông tin thanh toán
        [Display(Name = "Tạm tính")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Phí giao hàng")]
        public decimal ShippingFee { get; set; }

        [Display(Name = "Giảm giá")]
        public decimal Discount { get; set; }

        [Display(Name = "Tổng cộng")]
        public decimal Total { get; set; }

        [Display(Name = "Số dư ví")]
        public decimal WalletBalance { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; }

        // Thông tin giao hàng
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // Mã giảm giá
        [Display(Name = "Mã giảm giá")]
        public string? CouponCode { get; set; }
    }
}