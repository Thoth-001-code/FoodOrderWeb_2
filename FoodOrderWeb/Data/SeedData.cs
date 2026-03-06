using Microsoft.AspNetCore.Identity;
using FoodOrderWeb.Models;

namespace FoodOrderWeb.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Tạo roles (vai trò)
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Tạo tài khoản Admin mặc định
            string adminEmail = "admin@foodorder.com";
            string adminPassword = "Admin@123";

            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Tạo tài khoản User mẫu
            string userEmail = "user@example.com";
            string userPassword = "User@123";

            if (await userManager.FindByEmailAsync(userEmail) == null)
            {
                var normalUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    FullName = "Nguyễn Văn A",
                    Address = "123 Đường ABC, Quận 1, TP.HCM",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(normalUser, userPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                }
            }

            // Thêm dữ liệu mẫu cho FoodItems nếu chưa có
            if (!context.FoodItems.Any())
            {
                var foodItems = new List<FoodItem>
                {
                    new FoodItem
                    {
                        Name = "Phở bò",
                        Description = "Phở bò tái nạm, nước dùng đậm đà",
                        Price = 55000,
                        ImageUrl = "/images/foods/pho-bo.jpg",
                        IsAvailable = true,
                        CategoryId = 2, // Món chính
                        CreatedAt = DateTime.Now
                    },
                    new FoodItem
                    {
                        Name = "Cơm tấm sườn bì chả",
                        Description = "Cơm tấm với sườn nướng, bì, chả trứng",
                        Price = 45000,
                        ImageUrl = "/images/foods/com-tam.jpg",
                        IsAvailable = true,
                        CategoryId = 2,
                        CreatedAt = DateTime.Now
                    },
                    new FoodItem
                    {
                        Name = "Trà sữa trân châu",
                        Description = "Trà sữa truyền thống với trân châu đen",
                        Price = 35000,
                        ImageUrl = "/images/foods/tra-sua.jpg",
                        IsAvailable = true,
                        CategoryId = 3, // Đồ uống
                        CreatedAt = DateTime.Now
                    }
                };

                context.FoodItems.AddRange(foodItems);
                await context.SaveChangesAsync();
            }
        }
    }
}