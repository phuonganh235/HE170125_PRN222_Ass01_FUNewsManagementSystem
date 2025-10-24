using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASS1.Controllers
{
    public class AdminController : Controller
    {
        private readonly IGenericRepository<SystemAccount> _accountRepository;
        private readonly IAuthenticationService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IGenericRepository<SystemAccount> accountRepository,
            IAuthenticationService authService,
            IConfiguration configuration,
            ILogger<AdminController> logger)
        {
            _accountRepository = accountRepository;
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var accounts = await _accountRepository.GetAllAsync();
                return View(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts");
                return View(new List<SystemAccount>());
            }
        }

        public async Task<IActionResult> CheckAdmin()
        {
            try
            {
                var adminAccount = await _accountRepository.GetByPredicateAsync(
                    x => x.AccountEmail == "admin@FUNewsManagementSystem.org");

                if (adminAccount.Any())
                {
                    var admin = adminAccount.First();
                    ViewBag.AdminExists = true;
                    ViewBag.AdminInfo = $"Email: {admin.AccountEmail}, Role: {admin.AccountRole}, Name: {admin.AccountName}";
                }
                else
                {
                    ViewBag.AdminExists = false;
                    ViewBag.AdminInfo = "Admin account not found";
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking admin account");
                ViewBag.AdminExists = false;
                ViewBag.AdminInfo = $"Error: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> TestLogin()
        {
            try
            {
                // Test database admin login
                var dbResult = await _authService.LoginAsync("admin@FUNewsManagementSystem.org", "@@abc123@@");
                
                // Test appsettings admin login (simulate AccountController logic)
                var adminConfig = _configuration.GetSection("DefaultAdmin").Get<AdminAccountConfig>();
                bool isAppSettingsAdmin = adminConfig?.Email == "admin@FUNewsManagementSystem.org" && 
                                        adminConfig?.Password == "@@abc123@@";

                if (dbResult != null)
                {
                    ViewBag.LoginResult = "SUCCESS (Database)";
                    ViewBag.UserInfo = $"Database login: {dbResult.AccountName} (Role: {dbResult.AccountRole})";
                }
                else if (isAppSettingsAdmin)
                {
                    ViewBag.LoginResult = "SUCCESS (AppSettings)";
                    ViewBag.UserInfo = $"AppSettings admin: {adminConfig.AccountName} (Role: {adminConfig.AccountRole}) - Ready for login";
                }
                else
                {
                    ViewBag.LoginResult = "FAILED";
                    ViewBag.UserInfo = "Both database and appsettings admin login failed";
                }
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing admin login");
                ViewBag.LoginResult = "ERROR";
                ViewBag.UserInfo = $"Error: {ex.Message}";
                return View();
            }
        }
    }
}
