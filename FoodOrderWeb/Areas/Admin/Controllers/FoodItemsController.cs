using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodOrderWeb.Data;
using FoodOrderWeb.Models;
using FoodOrderWeb.ViewModels;

namespace FoodOrderWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class FoodItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FoodItemsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/FoodItems
        public async Task<IActionResult> Index()
        {
            var foodItems = await _context.FoodItems
                .Include(f => f.Category)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(foodItems);
        }

        // GET: Admin/FoodItems/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Admin/FoodItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FoodItemViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;

                // Xử lý upload hình ảnh
                if (model.ImageFile != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "foods");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }
                }

                var foodItem = new FoodItem
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    ImageUrl = uniqueFileName != null ? "/images/foods/" + uniqueFileName : null,
                    IsAvailable = model.IsAvailable,
                    CategoryId = model.CategoryId,
                    CreatedAt = DateTime.Now
                };

                _context.Add(foodItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thêm món ăn thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        // GET: Admin/FoodItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            var model = new FoodItemViewModel
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Price = foodItem.Price,
                IsAvailable = foodItem.IsAvailable,
                CategoryId = foodItem.CategoryId,
                ExistingImageUrl = foodItem.ImageUrl
            };

            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        // POST: Admin/FoodItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FoodItemViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var foodItem = await _context.FoodItems.FindAsync(id);
                    if (foodItem == null)
                    {
                        return NotFound();
                    }

                    // Xử lý upload hình ảnh mới
                    if (model.ImageFile != null)
                    {
                        // Xóa hình cũ nếu có
                        if (!string.IsNullOrEmpty(foodItem.ImageUrl))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath,
                                foodItem.ImageUrl.TrimStart('/').Replace("/", "\\"));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Upload hình mới
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "foods");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(fileStream);
                        }

                        foodItem.ImageUrl = "/images/foods/" + uniqueFileName;
                    }

                    // Cập nhật thông tin
                    foodItem.Name = model.Name;
                    foodItem.Description = model.Description;
                    foodItem.Price = model.Price;
                    foodItem.IsAvailable = model.IsAvailable;
                    foodItem.CategoryId = model.CategoryId;

                    _context.Update(foodItem);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật món ăn thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodItemExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.Categories.ToList();
            return View(model);
        }

        // POST: Admin/FoodItems/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var foodItem = await _context.FoodItems
                .Include(f => f.OrderItems)
                .Include(f => f.CartItems)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (foodItem == null)
            {
                return Json(new { success = false, message = "Không tìm thấy món ăn" });
            }

            // Kiểm tra xem món ăn đã được đặt chưa
            if (foodItem.OrderItems != null && foodItem.OrderItems.Any())
            {
                return Json(new { success = false, message = "Không thể xóa món ăn đã có đơn hàng" });
            }

            // Xóa hình ảnh
            if (!string.IsNullOrEmpty(foodItem.ImageUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath,
                    foodItem.ImageUrl.TrimStart('/').Replace("/", "\\"));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.FoodItems.Remove(foodItem);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa món ăn thành công" });
        }

        private bool FoodItemExists(int id)
        {
            return _context.FoodItems.Any(e => e.Id == id);
        }
    }
}