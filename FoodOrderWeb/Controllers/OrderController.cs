using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodOrderWeb.Data;
using FoodOrderWeb.Models;
using System.Security.Claims;

namespace FoodOrderWeb.Controllers
{
    [Authorize(Roles = "User")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ApplicationDbContext context, ILogger<OrderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Order/MyOrders
        public async Task<IActionResult> MyOrders(int page = 1, string status = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                int pageSize = 10;

                var query = _context.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderItems)
                    .AsQueryable();

                // Lọc theo trạng thái
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var orderStatus))
                {
                    query = query.Where(o => o.Status == orderStatus);
                    ViewBag.CurrentStatus = status;
                }

                // Sắp xếp
                query = query.OrderByDescending(o => o.OrderDate);

                // Phân trang
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Thống kê
                ViewBag.TotalOrders = totalItems;
                ViewBag.TotalSpent = await _context.Orders
                    .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
                    .SumAsync(o => o.TotalAmount);
                ViewBag.PendingCount = await _context.Orders
                    .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Pending);
                ViewBag.DeliveredCount = await _context.Orders
                    .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Delivered);
                ViewBag.CancelledCount = await _context.Orders
                    .CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Cancelled);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải đơn hàng của user");
                return View(new List<Order>());
            }
        }

        // GET: /Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.FoodItem)
                            .ThenInclude(f => f.Category)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    return NotFound();
                }

                // Lấy lịch sử giao dịch nếu thanh toán bằng ví
                if (order.PaymentMethod == PaymentMethod.Wallet)
                {
                    var transaction = await _context.Transactions
                        .Include(t => t.Wallet)
                        .FirstOrDefaultAsync(t => t.OrderId == order.Id);

                    ViewBag.Transaction = transaction;
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết đơn hàng {OrderId}", id);
                return NotFound();
            }
        }

        // POST: /Order/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Chỉ cho phép hủy đơn hàng ở trạng thái Pending
                if (order.Status != OrderStatus.Pending)
                {
                    return Json(new { success = false, message = "Chỉ có thể hủy đơn hàng đang chờ xác nhận" });
                }

                // Nếu đã thanh toán bằng ví thì hoàn tiền
                if (order.PaymentMethod == PaymentMethod.Wallet && order.IsPaid)
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        var wallet = await _context.Wallets
                            .FirstOrDefaultAsync(w => w.UserId == userId);

                        var balanceBefore = wallet.Balance;

                        // Hoàn tiền vào ví
                        wallet.Balance += order.TotalAmount;
                        wallet.LastUpdated = DateTime.Now;

                        // Ghi lịch sử giao dịch hoàn tiền
                        var walletTransaction = new Transaction
                        {
                            WalletId = wallet.Id,
                            Type = TransactionType.Refund,
                            Amount = order.TotalAmount,
                            BalanceBefore = balanceBefore,
                            BalanceAfter = wallet.Balance,
                            Description = $"Hoàn tiền đơn hàng #{order.Id}",
                            OrderId = order.Id,
                            CreatedAt = DateTime.Now
                        };
                        _context.Transactions.Add(walletTransaction);

                        order.Status = OrderStatus.Cancelled;
                        order.IsPaid = false;

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("User {UserId} đã hủy đơn hàng {OrderId} và được hoàn tiền", userId, order.Id);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                else
                {
                    order.Status = OrderStatus.Cancelled;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("User {UserId} đã hủy đơn hàng {OrderId}", userId, order.Id);
                }

                return Json(new { success = true, message = "Hủy đơn hàng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hủy đơn hàng {OrderId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi hủy đơn hàng" });
            }
        }

        // GET: /Order/Track/5
        [HttpGet]
        public async Task<IActionResult> Track(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.FoodItem)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi theo dõi đơn hàng {OrderId}", id);
                return NotFound();
            }
        }

        // GET: /Order/Reorder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Lấy giỏ hàng của user
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, CreatedAt = DateTime.Now };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                // Thêm các món từ đơn hàng cũ vào giỏ
                foreach (var orderItem in order.OrderItems)
                {
                    var existingItem = cart.CartItems?.FirstOrDefault(ci => ci.FoodItemId == orderItem.FoodItemId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += orderItem.Quantity;
                    }
                    else
                    {
                        var cartItem = new CartItem
                        {
                            CartId = cart.Id,
                            FoodItemId = orderItem.FoodItemId,
                            Quantity = orderItem.Quantity,
                            CreatedAt = DateTime.Now
                        };
                        _context.CartItems.Add(cartItem);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đã thêm vào giỏ hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đặt lại đơn hàng {OrderId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}