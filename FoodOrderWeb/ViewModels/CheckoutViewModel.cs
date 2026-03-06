using System.ComponentModel.DataAnnotations;
using FoodOrderWeb.Models;

namespace FoodOrderWeb.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        [Display(Name = "Tạm tính")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Phí giao hàng")]
        public decimal ShippingFee { get; set; } = 0;

        [Display(Name = "Giảm giá")]
        public decimal Discount { get; set; }

        [Display(Name = "Tổng cộng")]
        public decimal Total { get; set; }

        [Display(Name = "Số dư ví")]
        public decimal WalletBalance { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100, ErrorMessage = "Tên người nhận không được vượt quá 100 ký tự")]
        [Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Mã giảm giá")]
        public string? CouponCode { get; set; }

        [Display(Name = "Xuất hóa đơn")]
        public bool NeedInvoice { get; set; }

        [Display(Name = "Tên công ty")]
        public string? CompanyName { get; set; }

        [Display(Name = "Mã số thuế")]
        public string? TaxCode { get; set; }

        [Display(Name = "Địa chỉ công ty")]
        public string? CompanyAddress { get; set; }
    }

    public class CheckoutProcessViewModel
    {
        public int OrderId { get; set; }
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string Status { get; set; }
        public string ShippingAddress { get; set; }
        public string EstimatedDeliveryTime { get; set; }
    }
}