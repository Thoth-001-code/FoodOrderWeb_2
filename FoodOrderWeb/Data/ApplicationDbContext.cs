using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FoodOrderWeb.Models;

namespace FoodOrderWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet cho mỗi bảng
        public DbSet<Category> Categories { get; set; }
        public DbSet<FoodItem> FoodItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === CẤU HÌNH QUAN HỆ GIỮA CÁC BẢNG ===

            // User - Cart (1-1)
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId);

            // User - Wallet (1-1)
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Wallet)
                .WithOne(w => w.User)
                .HasForeignKey<Wallet>(w => w.UserId);

            // User - Orders (1-n)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId);

            // Cart - CartItems (1-n)
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // FoodItem - CartItems (1-n)
            modelBuilder.Entity<FoodItem>()
                .HasMany(f => f.CartItems)
                .WithOne(ci => ci.FoodItem)
                .HasForeignKey(ci => ci.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order - OrderItems (1-n)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // FoodItem - OrderItems (1-n)
            modelBuilder.Entity<FoodItem>()
                .HasMany(f => f.OrderItems)
                .WithOne(oi => oi.FoodItem)
                .HasForeignKey(oi => oi.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Wallet - Transactions (1-n)
            modelBuilder.Entity<Wallet>()
                .HasMany(w => w.Transactions)
                .WithOne(t => t.Wallet)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order - Transactions (1-n)
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Transactions)
                .WithOne(t => t.Order)
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category - FoodItems (1-n)
            modelBuilder.Entity<Category>()
                .HasMany(c => c.FoodItems)
                .WithOne(f => f.Category)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // === CẤU HÌNH KIỂU DỮ LIỆU CHO TIỀN TỆ ===

            // FoodItem.Price
            modelBuilder.Entity<FoodItem>()
                .Property(f => f.Price)
                .HasColumnType("decimal(18,2)");

            // Order - các trường tiền
            modelBuilder.Entity<Order>()
                .Property(o => o.Subtotal)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.Discount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Wallet.Balance
            modelBuilder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasColumnType("decimal(18,2)");

            // Transaction - các trường tiền
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.BalanceBefore)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .Property(t => t.BalanceAfter)
                .HasColumnType("decimal(18,2)");

            // === THÊM DỮ LIỆU MẪU (SEED DATA) ===

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Món khai vị", Description = "Các món ăn nhẹ trước bữa chính", CreatedAt = DateTime.Now },
                new Category { Id = 2, Name = "Món chính", Description = "Các món ăn chính", CreatedAt = DateTime.Now },
                new Category { Id = 3, Name = "Đồ uống", Description = "Nước ngọt, bia, rượu", CreatedAt = DateTime.Now },
                new Category { Id = 4, Name = "Tráng miệng", Description = "Các món ngọt sau bữa ăn", CreatedAt = DateTime.Now }
            );

            // === TẠO INDEX ĐỂ TỐI ƯU TRUY VẤN ===

            modelBuilder.Entity<FoodItem>()
                .HasIndex(f => f.CategoryId)
                .HasDatabaseName("IX_FoodItems_CategoryId");

            modelBuilder.Entity<FoodItem>()
                .HasIndex(f => f.IsAvailable)
                .HasDatabaseName("IX_FoodItems_IsAvailable");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId)
                .HasDatabaseName("IX_Orders_UserId");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.Status)
                .HasDatabaseName("IX_Orders_Status");

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderDate)
                .HasDatabaseName("IX_Orders_OrderDate");

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.WalletId)
                .HasDatabaseName("IX_Transactions_WalletId");

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.CreatedAt)
                .HasDatabaseName("IX_Transactions_CreatedAt");
        }

        internal object Entry(object cartItem)
        {
            throw new NotImplementedException();
        }
    }
}