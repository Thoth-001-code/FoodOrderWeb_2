using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodOrderWeb.Data;
using FoodOrderWeb.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FoodOrderWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy 8 món ăn nổi bật nhất (mới nhất)
                var foodItems = await _context.FoodItems
                    .Include(f => f.Category)
                    .Where(f => f.IsAvailable)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(8)
                    .ToListAsync();

                // Lấy danh sách danh mục cho menu
                var categories = await _context.Categories
                    .Include(c => c.FoodItems)
                    .ToListAsync();

                ViewBag.Categories = categories;

                // Lấy danh sách món ăn mới nhất
                var newItems = await _context.FoodItems
                    .Include(f => f.Category)
                    .Where(f => f.IsAvailable)
                    .OrderByDescending(f => f.CreatedAt)
                    .Take(4)
                    .ToListAsync();

                ViewBag.NewItems = newItems;

                // Nếu user đã đăng nhập và không phải admin, lấy số lượng giỏ hàng
                if (User.Identity.IsAuthenticated && !User.IsInRole("Admin"))
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var cart = await _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefaultAsync(c => c.UserId == userId);

                    ViewBag.CartCount = cart?.CartItems?.Sum(ci => ci.Quantity) ?? 0;
                }

                return View(foodItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang chủ");
                return View(new List<FoodItem>());
            }
        }

        // GET: /Home/About
        public IActionResult About()
        {
            ViewData["Message"] = "Về chúng tôi";
            return View();
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            ViewData["Message"] = "Liên hệ với chúng tôi";
            return View();
        }

        // POST: /Home/SendContact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendContact(string name, string email, string message)
        {
            // Xử lý gửi liên hệ (có thể lưu vào database hoặc gửi email)
            TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất.";
            return RedirectToAction("Contact");
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Search
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index");
            }

            var results = await _context.FoodItems
                .Include(f => f.Category)
                .Where(f => f.IsAvailable &&
                           (f.Name.Contains(keyword) ||
                            f.Description.Contains(keyword) ||
                            f.Category.Name.Contains(keyword)))
                .ToListAsync();

            ViewBag.Keyword = keyword;
            return View(results);
        }

        // GET: /Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}