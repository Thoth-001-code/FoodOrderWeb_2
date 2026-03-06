using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderWeb.Models
{
    public enum WalletStatus
    {
        Active = 0,
        Locked = 1
    }

    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        [Required]
        public WalletStatus Status { get; set; } = WalletStatus.Active;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Cập nhật lần cuối")]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Liên kết
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }
    }
}