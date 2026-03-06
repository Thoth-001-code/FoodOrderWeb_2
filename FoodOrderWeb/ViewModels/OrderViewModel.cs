using System.ComponentModel.DataAnnotations;
using FoodOrderWeb.Models;

namespace FoodOrderWeb.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string OrderCode => $"DH{Id:D6}";
        public DateTime OrderDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName => GetStatusName();
        public PaymentMethod PaymentMethod { get; set; }
        public string PaymentMethodName => PaymentMethod == PaymentMethod.Cash ? "Tiền mặt" : "Ví điện tử";
        public bool IsPaid { get; set; }
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string ReceiverName { get; set; }
        public string? Notes { get; set; }
        public string? CouponCode { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; } = new List<OrderItemViewModel>();

        private string GetStatusName()
        {
            return Status switch
            {
                OrderStatus.Pending => "Chờ xác nhận",
                OrderStatus.Confirmed => "Đã xác nhận",
                OrderStatus.Preparing => "Đang chuẩn bị",
                OrderStatus.Delivering => "Đang giao",
                OrderStatus.Delivered => "Đã giao",
                OrderStatus.Cancelled => "Đã hủy",
                _ => "Không xác định"
            };
        }

        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                OrderStatus.Pending => "bg-warning",
                OrderStatus.Confirmed => "bg-info",
                OrderStatus.Preparing => "bg-primary",
                OrderStatus.Delivering => "bg-secondary",
                OrderStatus.Delivered => "bg-success",
                OrderStatus.Cancelled => "bg-danger",
                _ => "bg-dark"
            };
        }
    }

    public class OrderItemViewModel
    {
        public int Id { get; set; }
        public int FoodItemId { get; set; }
        public string FoodItemName { get; set; }
        public string? FoodItemImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }

    public class OrderListViewModel
    {
        public IEnumerable<OrderViewModel> Orders { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public OrderStatus? CurrentStatus { get; set; }

        // Thống kê
        public int TotalOrders { get; set; }
        public int PendingCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int PreparingCount { get; set; }
        public int DeliveringCount { get; set; }
        public int DeliveredCount { get; set; }
        public int CancelledCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class OrderDetailViewModel
    {
        public OrderViewModel Order { get; set; }
        public TransactionViewModel? Transaction { get; set; }
        public List<OrderStatusHistoryViewModel> StatusHistory { get; set; } = new List<OrderStatusHistoryViewModel>();
    }

    public class OrderStatusHistoryViewModel
    {
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Note { get; set; }
        public string? ChangedBy { get; set; }
    }

    public class OrderStatusUpdateViewModel
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public OrderStatus NewStatus { get; set; }

        [StringLength(255)]
        public string? Note { get; set; }
    }

    public class OrderCancelViewModel
    {
        [Required]
        public int OrderId { get; set; }

        [StringLength(255)]
        [Display(Name = "Lý do hủy")]
        public string? Reason { get; set; }
    }

    public class OrderSearchViewModel
    {
        public string? Keyword { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PhoneNumber { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class OrderStatisticsViewModel
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int PreparingOrders { get; set; }
        public int DeliveringOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }

        public decimal TodayRevenue { get; set; }
        public decimal WeekRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<DailyRevenueViewModel> DailyRevenues { get; set; }
        public List<TopSellingItemViewModel> TopSellingItems { get; set; }
    }

    public class DailyRevenueViewModel
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TopSellingItemViewModel
    {
        public int FoodItemId { get; set; }
        public string FoodItemName { get; set; }
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class TransactionViewModel
    {
        public int Id { get; set; }
        public TransactionType Type { get; set; }
        public string TypeName => GetTypeName();
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        private string GetTypeName()
        {
            return Type switch
            {
                TransactionType.Deposit => "Nạp tiền",
                TransactionType.Payment => "Thanh toán",
                TransactionType.Refund => "Hoàn tiền",
                _ => "Không xác định"
            };
        }

        public string GetTypeBadgeClass()
        {
            return Type switch
            {
                TransactionType.Deposit => "bg-success",
                TransactionType.Payment => "bg-danger",
                TransactionType.Refund => "bg-info",
                _ => "bg-dark"
            };
        }
    }
}