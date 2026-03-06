using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderWeb.Models
{
    public enum TransactionType
    {
        Deposit = 0,     // Nạp tiền
        Payment = 1,     // Thanh toán
        Refund = 2       // Hoàn tiền
    }

    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int WalletId { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceBefore { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceAfter { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public int? OrderId { get; set; }  // Nếu là thanh toán đơn hàng

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Liên kết
        [ForeignKey("WalletId")]
        public virtual Wallet? Wallet { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}