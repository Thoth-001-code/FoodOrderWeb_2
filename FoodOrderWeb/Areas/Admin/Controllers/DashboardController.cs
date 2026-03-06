using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodOrderWeb.Data;
using FoodOrderWeb.Models;

namespace FoodOrderWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalFoodItems = await _context.FoodItems.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();

            // Doanh thu
            ViewBag.TodayRevenue = await _context.Orders
                .Where(o => o.OrderDate.Date == DateTime.Today && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            ViewBag.WeekRevenue = await _context.Orders
                .Where(o => o.OrderDate >= DateTime.Today.AddDays(-7) && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            ViewBag.MonthRevenue = await _context.Orders
                .Where(o => o.OrderDate.Month == DateTime.Now.Month
                    && o.OrderDate.Year == DateTime.Now.Year
                    && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            ViewBag.TotalRevenue = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);

            // Đơn hàng theo trạng thái
            ViewBag.PendingOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .CountAsync();

            ViewBag.ConfirmedOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Confirmed)
                .CountAsync();

            ViewBag.DeliveringOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivering)
                .CountAsync();

            ViewBag.DeliveredOrders = await _context.Orders
                .Where(o => o.Status == OrderStatus.Delivered)
                .CountAsync();

            // Đơn hàng gần đây
            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            // Top món ăn bán chạy
            var topFoodItems = await _context.OrderItems
                .Include(oi => oi.FoodItem)
                .GroupBy(oi => new { oi.FoodItemId, oi.FoodItem.Name })
                .Select(g => new
                {
                    FoodItemName = g.Key.Name,
                    TotalQuantity = g.Sum(oi => oi.Quantity),
                    TotalRevenue = g.Sum(oi => oi.Quantity * oi.Price)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentOrders = recentOrders;
            ViewBag.TopFoodItems = topFoodItems;

            return View();
        }
    }
}