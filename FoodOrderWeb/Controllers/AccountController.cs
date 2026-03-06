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
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        // ============= ĐĂNG KÝ =============
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FullName = model.FullName,
                        PhoneNumber = model.PhoneNumber,
                        Address = model.Address,
                        RegistrationDate = DateTime.Now
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User đăng ký thành công: {Email}", model.Email);

                        // Gán role User mặc định
                        await _userManager.AddToRoleAsync(user, "User");

                        // Tạo ví điện tử cho user mới
                        var wallet = new Wallet
                        {
                            UserId = user.Id,
                            Balance = 0,
                            Status = WalletStatus.Active,
                            CreatedAt = DateTime.Now,
                            LastUpdated = DateTime.Now
                        };
                        _context.Wallets.Add(wallet);

                        // Tạo giỏ hàng cho user mới
                        var cart = new Cart
                        {
                            UserId = user.Id,
                            CreatedAt = DateTime.Now
                        };
                        _context.Carts.Add(cart);

                        await _context.SaveChangesAsync();

                        // Đăng nhập tự động
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                        return RedirectToAction("Index", "Home");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        _logger.LogWarning("Lỗi đăng ký: {Error}", error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi đăng ký user");
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng ký");
                }
            }

            return View(model);
        }

        // ============= ĐĂNG NHẬP =============
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        model.Email,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: true
                    );

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User đăng nhập thành công: {Email}", model.Email);

                        // Cập nhật last login
                        var user = await _userManager.FindByEmailAsync(model.Email);
                        // user.LastLoginAt = DateTime.Now;
                        await _userManager.UpdateAsync(user);

                        return RedirectToLocal(returnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("Tài khoản bị khóa: {Email}", model.Email);
                        return View("Lockout");
                    }

                    ModelState.AddModelError(string.Empty, "Đăng nhập không thành công. Vui lòng kiểm tra lại email và mật khẩu.");
                    _logger.LogWarning("Đăng nhập thất bại cho email: {Email}", model.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi đăng nhập");
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình đăng nhập");
                }
            }

            return View(model);
        }

        // ============= ĐĂNG XUẤT =============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User đăng xuất");
            return RedirectToAction("Index", "Home");
        }

        // ============= PROFILE =============
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            // Lấy thống kê
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.TotalOrders = await _context.Orders.CountAsync(o => o.UserId == userId);
            ViewBag.TotalSpent = await _context.Orders
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
                .SumAsync(o => o.TotalAmount);
            ViewBag.MemberSince = user.RegistrationDate.ToString("dd/MM/yyyy");

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    user.FullName = model.FullName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                        return RedirectToAction(nameof(Profile));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật profile");
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật thông tin");
                }
            }

            return View(model);
        }

        // ============= ĐỔI MẬT KHẨU =============
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                    if (result.Succeeded)
                    {
                        await _signInManager.RefreshSignInAsync(user);
                        TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                        return RedirectToAction(nameof(Profile));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi đổi mật khẩu");
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi đổi mật khẩu");
                }
            }

            return View(model);
        }

        // ============= FORGOT PASSWORD =============
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // Send email with reset link
                // var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, protocol: HttpContext.Request.Scheme);
                // await _emailSender.SendEmailAsync(model.Email, "Reset Password", $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");

                return View("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        // ============= RESET PASSWORD =============
        [HttpGet]
        public IActionResult ResetPassword(string token = null, string email = null)
        {
            if (token == null || email == null)
            {
                return BadRequest("Invalid password reset token");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        // ============= CONFIRM EMAIL =============
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // ============= ACCESS DENIED =============
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ============= EXTERNAL LOGIN =============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["ProviderDisplayName"] = info.ProviderDisplayName;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email, FullName = name });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("Error");
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName
                };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        // Tạo ví và giỏ hàng cho user mới
                        var wallet = new Wallet
                        {
                            UserId = user.Id,
                            Balance = 0,
                            Status = WalletStatus.Active,
                            CreatedAt = DateTime.Now,
                            LastUpdated = DateTime.Now
                        };
                        _context.Wallets.Add(wallet);

                        var cart = new Cart
                        {
                            UserId = user.Id,
                            CreatedAt = DateTime.Now
                        };
                        _context.Carts.Add(cart);

                        await _context.SaveChangesAsync();

                        return RedirectToLocal(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // ============= HELPER METHODS =============
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}