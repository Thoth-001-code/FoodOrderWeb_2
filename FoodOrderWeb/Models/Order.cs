using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderWeb.Models
{
    public enum OrderStatus
    {
        Pending = 0,        // Chờ xác nhận
        Confirmed = 1,       // Đã xác nhận
        Preparing = 2,       // Đang chuẩn bị
        Delivering = 3,      // Đang giao
        Delivered = 4,       // Đã giao
        Cancelled = 5        // Đã hủy
    }

    public enum PaymentMethod
    {
        Cash = 0,            // Tiền mặt
        Wallet = 1           // Ví điện tử
    }

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Ngày đặt")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        // THÊM CÁC THUỘC TÍNH MỚI
        [Display(Name = "Tạm tính")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Display(Name = "Phí giao hàng")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; }

        [Display(Name = "Giảm giá")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Required]
        [Display(Name = "Tổng tiền")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Mã giảm giá")]
        [StringLength(50)]
        public string? CouponCode { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        [Display(Name = "Phương thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "Đã thanh toán")]
        public bool IsPaid { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // Thông tin giao hàng
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Tên người nhận")]
        public string ReceiverName { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<OrderItem>? OrderItems { get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }
    }
}