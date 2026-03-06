using FoodOrderWeb.Data;
using FoodOrderWeb.Models;
using FoodOrderWeb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodOrderWeb.Controllers
{
    [Authorize(Roles = "User")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, ILogger<CartController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.FoodItem)
                            .ThenInclude(f => f.Category)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    cart = new Cart { UserId = userId, CreatedAt = DateTime.Now };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var cartItems = cart.CartItems?.ToList() ?? new List<CartItem>();
                var subtotal = cartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price);
                var discount = 0m; // Có thể thêm logic giảm giá sau
                var total = subtotal - discount;

                ViewBag.Subtotal = subtotal;
                ViewBag.Discount = discount;
                ViewBag.Total = total;
                ViewBag.ItemCount = cartItems.Sum(ci => ci.Quantity);

                return View(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải giỏ hàng");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải giỏ hàng";
                return View(new List<CartItem>());
            }
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int foodItemId, int quantity = 1)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Kiểm tra món ăn có tồn tại không
                var foodItem = await _context.FoodItems
                    .FirstOrDefaultAsync(f => f.Id == foodItemId && f.IsAvailable);

                if (foodItem == null)
                {
                    return Json(new { success = false, message = "Món ăn không tồn tại hoặc đã hết hàng" });
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

                    // Load lại cart để có CartItems
                    cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == userId);
                }

                // Kiểm tra món đã có trong giỏ chưa
                var cartItem = cart.CartItems?.FirstOrDefault(ci => ci.FoodItemId == foodItemId);

                if (cartItem != null)
                {
                    // Kiểm tra số lượng không vượt quá 10
                    if (cartItem.Quantity + quantity > 10)
                    {
                        return Json(new { success = false, message = "Số lượng mỗi món không được vượt quá 10" });
                    }
                    cartItem.Quantity += quantity;
                }
                else
                {
                    // Thêm mới
                    cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        FoodItemId = foodItemId,
                        Quantity = quantity,
                        CreatedAt = DateTime.Now
                    };
                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                // Đếm tổng số lượng và tổng tiền trong giỏ
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.FoodItem)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                var totalQuantity = cart.CartItems?.Sum(ci => ci.Quantity) ?? 0;
                var totalPrice = cart.CartItems?.Sum(ci => ci.Quantity * ci.FoodItem.Price) ?? 0;

                _logger.LogInformation("User {UserId} đã thêm món {FoodItemId} vào giỏ hàng", userId, foodItemId);

                return Json(new
                {
                    success = true,
                    message = "Đã thêm vào giỏ hàng",
                    cartCount = totalQuantity,
                    cartTotal = totalPrice.ToString("#,0")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm vào giỏ hàng");
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm vào giỏ hàng" });
            }
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.FoodItem)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy món trong giỏ" });
                }

                // Kiểm tra quyền sở hữu
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cart = await _context.Carts
                    .FirstOrDefaultAsync(c => c.Id == cartItem.CartId);

                if (cart.UserId != userId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền thao tác với giỏ hàng này" });
                }

                if (quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else if (quantity > 10)
                {
                    return Json(new { success = false, message = "Số lượng không được vượt quá 10" });
                }
                else
                {
                    cartItem.Quantity = quantity;
                }

                await _context.SaveChangesAsync();

                // Tính lại tổng tiền
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.FoodItem)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                var cartTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price);
                var itemTotal = quantity * cartItem.FoodItem.Price;
                var cartCount = cart.CartItems.Sum(ci => ci.Quantity);

                return Json(new
                {
                    success = true,
                    cartTotal = cartTotal.ToString("#,0"),
                    itemTotal = itemTotal.ToString("#,0"),
                    cartCount = cartCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật số lượng");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật số lượng" });
            }
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

                if (cartItem == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy món trong giỏ" });
                }

                // Kiểm tra quyền sở hữu
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (cartItem.Cart.UserId != userId)
                {
                    return Json(new { success = false, message = "Bạn không có quyền thao tác với giỏ hàng này" });
                }

                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();

                // Tính lại tổng tiền
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.FoodItem)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                var cartTotal = cart.CartItems?.Sum(ci => ci.Quantity * ci.FoodItem.Price) ?? 0;
                var cartCount = cart.CartItems?.Sum(ci => ci.Quantity) ?? 0;

                _logger.LogInformation("User {UserId} đã xóa món khỏi giỏ hàng", userId);

                return Json(new
                {
                    success = true,
                    message = "Đã xóa món khỏi giỏ",
                    cartTotal = cartTotal.ToString("#,0"),
                    cartCount = cartCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa món khỏi giỏ");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa món khỏi giỏ" });
            }
        }

        // GET: /Cart/GetCartCount
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                var count = cart?.CartItems?.Sum(ci => ci.Quantity) ?? 0;

                return Json(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy số lượng giỏ hàng");
                return Json(0);
            }
        }

        // GET: /Cart/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Lấy giỏ hàng
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.FoodItem)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                {
                    return RedirectToAction("Index");
                }

                // Lấy thông tin user
                var user = await _userManager.GetUserAsync(User);

                // Lấy thông tin ví
                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserId == userId);

                var model = new CheckoutViewModel
                {
                    CartItems = cart.CartItems.ToList(),
                    Subtotal = cart.CartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price),
                    ShippingFee = 0, // Miễn phí ship
                    Discount = 0,
                    Total = cart.CartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price),
                    WalletBalance = wallet?.Balance ?? 0,
                    ReceiverName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    ShippingAddress = user.Address,
                    PaymentMethod = PaymentMethod.Cash
                };

                // Kiểm tra xem có mã giảm giá không
                // if (TempData["CouponCode"] != null)
                // {
                //     model.CouponCode = TempData["CouponCode"].ToString();
                //     model.Discount = CalculateDiscount(model.CouponCode, model.Subtotal);
                //     model.Total = model.Subtotal + model.ShippingFee - model.Discount;
                // }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang thanh toán");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang thanh toán";
                return RedirectToAction("Index");
            }
        }

        // POST: /Cart/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Load lại dữ liệu
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.FoodItem)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                model.CartItems = cart.CartItems.ToList();
                model.Subtotal = cart.CartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price);
                model.ShippingFee = 0;
                model.Total = model.Subtotal - model.Discount;

                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
                model.WalletBalance = wallet?.Balance ?? 0;

                return View("Checkout", model);
            }

            var userId2 = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy giỏ hàng
            var cart2 = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.FoodItem)
                .FirstOrDefaultAsync(c => c.UserId == userId2);

            if (cart2 == null || !cart2.CartItems.Any())
            {
                return RedirectToAction("Index");
            }

            var subtotal = cart2.CartItems.Sum(ci => ci.Quantity * ci.FoodItem.Price);
            var total = subtotal + model.ShippingFee - model.Discount;

            // Kiểm tra số dư nếu thanh toán bằng ví
            if (model.PaymentMethod == PaymentMethod.Wallet)
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId2);
                if (wallet == null || wallet.Balance < total)
                {
                    ModelState.AddModelError("", "Số dư trong ví không đủ để thanh toán");

                    // Load lại dữ liệu
                    var user = await _userManager.GetUserAsync(User);
                    model.CartItems = cart2.CartItems.ToList();
                    model.Subtotal = subtotal;
                    model.Total = total;
                    model.WalletBalance = wallet?.Balance ?? 0;
                    model.ReceiverName = user.FullName;
                    model.PhoneNumber = user.PhoneNumber;
                    model.ShippingAddress = user.Address;

                    return View("Checkout", model);
                }
            }

            // Sử dụng transaction để đảm bảo tính toàn vẹn dữ liệu
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Tạo đơn hàng
                var order = new Order
                {
                    UserId = userId2,
                    OrderDate = DateTime.Now,
                    Subtotal = subtotal,
                    ShippingFee = model.ShippingFee,
                    Discount = model.Discount,
                    TotalAmount = total,
                    Status = OrderStatus.Pending,
                    PaymentMethod = model.PaymentMethod,
                    IsPaid = model.PaymentMethod == PaymentMethod.Wallet,
                    ShippingAddress = model.ShippingAddress,
                    PhoneNumber = model.PhoneNumber,
                    ReceiverName = model.ReceiverName,
                    Notes = model.Notes,
                    CouponCode = model.CouponCode
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Tạo chi tiết đơn hàng
                foreach (var cartItem in cart2.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        FoodItemId = cartItem.FoodItemId,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.FoodItem.Price
                    };
                    _context.OrderItems.Add(orderItem);
                }

                // Xử lý thanh toán bằng ví
                if (model.PaymentMethod == PaymentMethod.Wallet)
                {
                    var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId2);
                    var balanceBefore = wallet.Balance;

                    // Trừ tiền trong ví
                    wallet.Balance -= total;
                    wallet.LastUpdated = DateTime.Now;

                    // Ghi lịch sử giao dịch
                    var walletTransaction = new Transaction
                    {
                        WalletId = wallet.Id,
                        Type = TransactionType.Payment,
                        Amount = total,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = wallet.Balance,
                        Description = $"Thanh toán đơn hàng #{order.Id}",
                        OrderId = order.Id,
                        CreatedAt = DateTime.Now
                    };
                    _context.Transactions.Add(walletTransaction);
                }

                // Xóa giỏ hàng
                _context.CartItems.RemoveRange(cart2.CartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("User {UserId} đã đặt hàng thành công, OrderId: {OrderId}", userId2, order.Id);

                TempData["SuccessMessage"] = "Đặt hàng thành công!";
                return RedirectToAction("OrderSuccess", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi đặt hàng cho user {UserId}", userId2);

                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.");

                // Load lại dữ liệu
                var user = await _userManager.GetUserAsync(User);
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId2);

                model.CartItems = cart2.CartItems.ToList();
                model.Subtotal = subtotal;
                model.Total = total;
                model.WalletBalance = wallet?.Balance ?? 0;
                model.ReceiverName = user.FullName;
                model.PhoneNumber = user.PhoneNumber;
                model.ShippingAddress = user.Address;

                return View("Checkout", model);
            }
        }

        // GET: /Cart/OrderSuccess
        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int orderId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.FoodItem)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang đặt hàng thành công");
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: /Cart/ApplyCoupon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCoupon(string couponCode)
        {
            // Kiểm tra mã giảm giá
            // var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == couponCode && c.IsActive && c.ExpiryDate >= DateTime.Now);
            // if (coupon != null)
            // {
            //     TempData["CouponCode"] = couponCode;
            //     TempData["CouponMessage"] = "Áp dụng mã giảm giá thành công!";
            // }
            // else
            // {
            //     TempData["CouponError"] = "Mã giảm giá không hợp lệ hoặc đã hết hạn";
            // }

            return RedirectToAction("Checkout");
        }

        private UserManager<ApplicationUser> _userManager;
    }
}