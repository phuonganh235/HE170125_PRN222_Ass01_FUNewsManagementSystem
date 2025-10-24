using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ASS1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<AccountController> _logger;
        private readonly AdminAccountConfig _adminConfig;

        public AccountController(
            IAuthenticationService authService,
            IOptions<AdminAccountConfig> adminConfig,
            ILogger<AccountController> logger)
        {
            _authService = authService;
            _adminConfig = adminConfig.Value;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // First check if it's the admin account from appsettings
            _logger.LogInformation("Checking appsettings admin: Email={Email}, Password={Password}", model.Email, model.Password);
            _logger.LogInformation("Appsettings config: Email={ConfigEmail}, Password={ConfigPassword}", _adminConfig?.Email, _adminConfig?.Password);
            
            if (_adminConfig == null)
            {
                _logger.LogWarning("AdminConfig is null - appsettings not loaded properly");
            }
            
            if (model.Email == _adminConfig?.Email && model.Password == _adminConfig?.Password)
            {
                _logger.LogInformation("Appsettings admin credentials match - creating session");
                // Create admin session directly
                var adminSession = new UserSession
                {
                    AccountId = 999, // Special ID for appsettings admin
                    AccountName = _adminConfig.AccountName,
                    AccountEmail = _adminConfig.Email,
                    AccountRole = _adminConfig.AccountRole,
                    RoleName = "Admin",
                    IsAuthenticated = true
                };

                // Store in session
                var httpContext = HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.SetString("UserSession", System.Text.Json.JsonSerializer.Serialize(adminSession));
                }

                _logger.LogInformation("Admin user {Email} logged in successfully from appsettings", model.Email);
                return RedirectToAction("Index", "Home");
            }

            // If not admin, try regular authentication
            _logger.LogInformation("Not appsettings admin - trying regular authentication");
            var user = await _authService.LoginAsync(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            _logger.LogInformation("User {Email} logged in successfully", model.Email);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            return View(user);
        }

        public IActionResult TestAdminConfig()
        {
            ViewBag.AdminConfig = _adminConfig;
            ViewBag.IsNull = _adminConfig == null;
            return View();
        }
    }
}
