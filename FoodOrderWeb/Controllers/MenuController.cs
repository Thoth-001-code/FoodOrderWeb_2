using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodOrderWeb.Data;
using FoodOrderWeb.Models;
using System.Security.Claims;

namespace FoodOrderWeb.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MenuController> _logger;

        public MenuController(ApplicationDbContext context, ILogger<MenuController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Menu
        public async Task<IActionResult> Index(int? categoryId, string searchString, string sortOrder, int page = 1)
        {
            int pageSize = 12; // Số món mỗi trang

            // Lấy danh sách danh mục
            ViewBag.Categories = await _context.Categories.ToListAsync();

            // Query món ăn
            var query = _context.FoodItems
                .Include(f => f.Category)
                .Where(f => f.IsAvailable)
                .AsQueryable();

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                query = query.Where(f => f.CategoryId == categoryId);
                ViewBag.SelectedCategory = categoryId;
            }

            // Tìm kiếm theo tên
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(f => f.Name.Contains(searchString) ||
                                         f.Description.Contains(searchString));
                ViewBag.SearchString = searchString;
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "price_asc":
                    query = query.OrderBy(f => f.Price);
                    ViewBag.CurrentSort = "price_asc";
                    break;
                case "price_desc":
                    query = query.OrderByDescending(f => f.Price);
                    ViewBag.CurrentSort = "price_desc";
                    break;
                case "name_asc":
                    query = query.OrderBy(f => f.Name);
                    ViewBag.CurrentSort = "name_asc";
                    break;
                case "name_desc":
                    query = query.OrderByDescending(f => f.Name);
                    ViewBag.CurrentSort = "name_desc";
                    break;
                default:
                    query = query.OrderByDescending(f => f.CreatedAt);
                    ViewBag.CurrentSort = "newest";
                    break;
            }

            // Phân trang
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var foodItems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalItems = totalItems;

            // Lấy số lượng giỏ hàng nếu user đã đăng nhập
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

        // GET: /Menu/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var foodItem = await _context.FoodItems
                    .Include(f => f.Category)
                    .FirstOrDefaultAsync(f => f.Id == id);

                if (foodItem == null)
                {
                    return NotFound();
                }

                // Lấy món ăn liên quan (cùng danh mục)
                var relatedItems = await _context.FoodItems
                    .Include(f => f.Category)
                    .Where(f => f.CategoryId == foodItem.CategoryId &&
                                f.Id != id &&
                                f.IsAvailable)
                    .Take(4)
                    .ToListAsync();

                ViewBag.RelatedItems = relatedItems;

                // Lấy đánh giá (nếu có)
                // ViewBag.Reviews = await _context.Reviews.Where(r => r.FoodItemId == id).ToListAsync();

                return View(foodItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xem chi tiết món ăn {Id}", id);
                return NotFound();
            }
        }

        // GET: /Menu/GetSuggestions
        [HttpGet]
        public async Task<IActionResult> GetSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new List<object>());
            }

            var suggestions = await _context.FoodItems
                .Where(f => f.IsAvailable && f.Name.Contains(term))
                .Select(f => new {
                    id = f.Id,
                    name = f.Name,
                    price = f.Price.ToString("#,0") + "đ",
                    image = f.ImageUrl ?? "/images/default-food.jpg"
                })
                .Take(5)
                .ToListAsync();

            return Json(suggestions);
        }

        // GET: /Menu/Popular
        public async Task<IActionResult> Popular()
        {
            var popularItems = await _context.OrderItems
                .Include(oi => oi.FoodItem)
                    .ThenInclude(f => f.Category)
                .GroupBy(oi => new { oi.FoodItemId, oi.FoodItem.Name, oi.FoodItem.Price, oi.FoodItem.ImageUrl })
                .Select(g => new
                {
                    FoodItem = new FoodItem
                    {
                        Id = g.Key.FoodItemId,
                        Name = g.Key.Name,
                        Price = g.Key.Price,
                        ImageUrl = g.Key.ImageUrl
                    },
                    OrderCount = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.OrderCount)
                .Take(12)
                .Select(x => x.FoodItem)
                .ToListAsync();

            return View(popularItems);
        }
    }
}