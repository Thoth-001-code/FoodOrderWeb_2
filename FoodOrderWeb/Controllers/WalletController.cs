using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodOrderWeb.Data;
using FoodOrderWeb.Models;
using FoodOrderWeb.ViewModels;
using System.Security.Claims;

namespace FoodOrderWeb.Controllers
{
    [Authorize(Roles = "User")]
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WalletController> _logger;

        public WalletController(ApplicationDbContext context, ILogger<WalletController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Wallet
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    wallet = new Wallet
                    {
                        UserId = userId,
                        Balance = 0,
                        Status = WalletStatus.Active,
                        CreatedAt = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };
                    _context.Wallets.Add(wallet);
                    await _context.SaveChangesAsync();
                }

                // Lấy 10 giao dịch gần nhất
                var transactions = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id)
                    .Include(t => t.Order)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(10)
                    .ToListAsync();

                // Thống kê
                ViewBag.TotalDeposit = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id && t.Type == TransactionType.Deposit)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                ViewBag.TotalSpent = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id && t.Type == TransactionType.Payment)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                ViewBag.TransactionCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id);

                ViewBag.Transactions = transactions;

                return View(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải ví điện tử");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải ví điện tử";
                return View(new Wallet());
            }
        }

        // GET: /Wallet/Deposit
        [HttpGet]
        public IActionResult Deposit()
        {
            return View(new DepositViewModel());
        }

        // POST: /Wallet/Deposit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(DepositViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return NotFound();
                }

                var balanceBefore = wallet.Balance;

                // Cộng tiền vào ví
                wallet.Balance += model.Amount;
                wallet.LastUpdated = DateTime.Now;

                // Ghi lịch sử giao dịch
                var transaction = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = TransactionType.Deposit,
                    Amount = model.Amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,
                    Description = model.Description ?? "Nạp tiền vào ví",
                    CreatedAt = DateTime.Now
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} đã nạp {Amount} vào ví", userId, model.Amount);

                TempData["SuccessMessage"] = $"Nạp thành công {model.Amount.ToString("#,0")}đ vào ví!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nạp tiền vào ví");
                ModelState.AddModelError("", "Có lỗi xảy ra khi nạp tiền. Vui lòng thử lại.");
                return View(model);
            }
        }

        // GET: /Wallet/Transactions
        [HttpGet]
        public async Task<IActionResult> Transactions(int page = 1, int pageSize = 20, string type = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return NotFound();
                }

                // Lấy transactions, không phải fooditems
                var query = _context.Transactions
                    .Where(t => t.WalletId == wallet.Id)
                    .Include(t => t.Order)
                    .AsQueryable();

                // Lọc theo loại giao dịch
                if (!string.IsNullOrEmpty(type) && Enum.TryParse<TransactionType>(type, out var transactionType))
                {
                    query = query.Where(t => t.Type == transactionType);
                    ViewBag.CurrentType = type;
                }

                // Sắp xếp
                query = query.OrderByDescending(t => t.CreatedAt);

                // Phân trang
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var transactions = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Thống kê theo loại
                ViewBag.DepositCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id && t.Type == TransactionType.Deposit);
                ViewBag.PaymentCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id && t.Type == TransactionType.Payment);
                ViewBag.RefundCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id && t.Type == TransactionType.Refund);

                ViewBag.TotalDeposit = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id && t.Type == TransactionType.Deposit)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                ViewBag.TotalSpent = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id && t.Type == TransactionType.Payment)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                ViewBag.TotalRefund = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id && t.Type == TransactionType.Refund)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                // Thống kê theo thời gian
                var today = DateTime.Today;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
                var startOfMonth = new DateTime(today.Year, today.Month, 1);

                ViewBag.TodayCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id && t.CreatedAt.Date == today);
                ViewBag.TodayAmount = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id && t.CreatedAt.Date == today)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                ViewBag.WeekCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id && t.CreatedAt >= startOfWeek);
                ViewBag.WeekAmount = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id && t.CreatedAt >= startOfWeek)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                ViewBag.MonthCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id && t.CreatedAt >= startOfMonth);
                ViewBag.MonthAmount = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id && t.CreatedAt >= startOfMonth)
                    .SumAsync(t => (decimal?)t.Amount) ?? 0;

                ViewBag.AvgTransaction = transactions.Any()
                    ? transactions.Average(t => t.Amount)
                    : 0;

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.Wallet = wallet;

                // TRẢ VỀ LIST<TRANSACTION>
                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lịch sử giao dịch");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải lịch sử giao dịch";
                return View(new List<Transaction>());
            }
        }

        // GET: /Wallet/Transaction/5
        [HttpGet]
        public async Task<IActionResult> Transaction(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return NotFound();
                }

                var transaction = await _context.Transactions
                    .Include(t => t.Order)
                    .Include(t => t.Wallet)
                    .FirstOrDefaultAsync(t => t.Id == id && t.WalletId == wallet.Id);

                if (transaction == null)
                {
                    return NotFound();
                }

                return PartialView("_TransactionDetail", transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết giao dịch");
                return NotFound();
            }
        }

        // POST: /Wallet/Withdraw (nếu có chức năng rút tiền)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(decimal amount)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                if (wallet == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ví" });
                }

                if (wallet.Balance < amount)
                {
                    return Json(new { success = false, message = "Số dư không đủ" });
                }

                var balanceBefore = wallet.Balance;

                wallet.Balance -= amount;
                wallet.LastUpdated = DateTime.Now;

                var transaction = new Transaction
                {
                    WalletId = wallet.Id,
                    Type = TransactionType.Refund,
                    Amount = amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,
                    Description = "Rút tiền từ ví",
                    CreatedAt = DateTime.Now
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Rút tiền thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi rút tiền");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}