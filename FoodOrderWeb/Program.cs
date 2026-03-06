using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using FoodOrderWeb.Data;
using FoodOrderWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình DbContext (kết nối database)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity (xác thực người dùng)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Cấu hình mật khẩu
    options.Password.RequireDigit = true;           // Có số
    options.Password.RequiredLength = 6;            // Độ dài tối thiểu
    options.Password.RequireNonAlphanumeric = false;// Không cần ký tự đặc biệt
    options.Password.RequireUppercase = true;       // Có chữ hoa
    options.Password.RequireLowercase = true;       // Có chữ thường

    // Cấu hình khóa tài khoản
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // Cấu hình User
    options.User.RequireUniqueEmail = true;         // Email duy nhất
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cấu hình Cookie đăng nhập
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";           // Trang đăng nhập
    options.LogoutPath = "/Account/Logout";         // Trang đăng xuất
    options.AccessDeniedPath = "/Account/AccessDenied"; // Trang từ chối truy cập
    options.ExpireTimeSpan = TimeSpan.FromDays(7);  // Cookie tồn tại 7 ngày
    options.SlidingExpiration = true;
});

// Thêm Session (lưu giỏ hàng tạm thời)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session tồn tại 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();        // Cho phép dùng file tĩnh (css, js, images)
app.UseRouting();
app.UseSession();            // Dùng Session
app.UseAuthentication();     // Dùng xác thực
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Tạo dữ liệu mẫu khi chạy lần đầu
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.Initialize(services);
}
 
app.Run();