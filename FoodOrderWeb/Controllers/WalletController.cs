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

                // Lấy thông tin ví
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                // Nếu chưa có ví thì tạo mới (dự phòng)
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
                    .SumAsync(t => t.Amount);

                ViewBag.TotalSpent = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id && t.Type == TransactionType.Payment)
                    .SumAsync(t => t.Amount);

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
                    Description = model.Description ?? $"Nạp tiền vào ví",
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

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalItems = totalItems;
                ViewBag.Wallet = wallet;

                // Thống kê theo loại
                ViewBag.DepositCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id && t.Type == TransactionType.Deposit);
                ViewBag.PaymentCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id && t.Type == TransactionType.Payment);
                ViewBag.RefundCount = await _context.Transactions
                    .CountAsync(t => t.WalletId == wallet.Id && t.Type == TransactionType.Refund);

                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lịch sử giao dịch");
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

                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết giao dịch {TransactionId}", id);
                return NotFound();
            }
        }

        // GET: /Wallet/Statement
        [HttpGet]
        public async Task<IActionResult> Statement(DateTime? fromDate, DateTime? toDate)
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

                // Mặc định là 30 ngày gần nhất
                fromDate = fromDate ?? DateTime.Now.AddDays(-30);
                toDate = toDate ?? DateTime.Now;

                var transactions = await _context.Transactions
                    .Where(t => t.WalletId == wallet.Id &&
                                t.CreatedAt.Date >= fromDate.Value.Date &&
                                t.CreatedAt.Date <= toDate.Value.Date)
                    .OrderBy(t => t.CreatedAt)
                    .ToListAsync();

                ViewBag.FromDate = fromDate.Value.ToString("yyyy-MM-dd");
                ViewBag.ToDate = toDate.Value.ToString("yyyy-MM-dd");
                ViewBag.Wallet = wallet;
                ViewBag.OpeningBalance = await GetOpeningBalance(wallet.Id, fromDate.Value);
                ViewBag.ClosingBalance = wallet.Balance;
                ViewBag.TotalDeposit = transactions.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount);
                ViewBag.TotalPayment = transactions.Where(t => t.Type == TransactionType.Payment).Sum(t => t.Amount);

                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sao kê");
                return RedirectToAction("Index");
            }
        }

        // Helper method to get opening balance
        private async Task<decimal> GetOpeningBalance(int walletId, DateTime date)
        {
            var lastTransaction = await _context.Transactions
                .Where(t => t.WalletId == walletId && t.CreatedAt.Date < date.Date)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            return lastTransaction?.BalanceAfter ?? 0;
        }
    }
}