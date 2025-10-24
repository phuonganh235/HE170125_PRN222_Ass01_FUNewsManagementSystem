using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace ASS1.Services
{
    public interface IAuthenticationService
    {
        Task<UserSession?> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<UserSession?> GetCurrentUserAsync();
        bool IsAuthenticated();
        bool HasRole(int role);
        bool IsAdmin();
        bool IsStaff();
        bool IsLecturer();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IGenericRepository<SystemAccount> _accountRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationService> _logger;

        private const string SESSION_KEY = "UserSession";

        public AuthenticationService(
            IGenericRepository<SystemAccount> accountRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<AuthenticationService> logger)
        {
            _accountRepository = accountRepository;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserSession?> LoginAsync(string email, string password)
        {
            try
            {
                var accounts = await _accountRepository.GetByPredicateAsync(
                    x => x.AccountEmail == email);

                if (!accounts.Any())
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", email);
                    return null;
                }

                var account = accounts.First();

                if (account.AccountPassword != password)
                {
                    _logger.LogWarning("Invalid password for email: {Email}", email);
                    return null;
                }

                var userSession = new UserSession
                {
                    AccountId = account.AccountId,
                    AccountName = account.AccountName ?? string.Empty,
                    AccountEmail = account.AccountEmail ?? string.Empty,
                    AccountRole = account.AccountRole ?? 0,
                    RoleName = GetRoleName(account.AccountRole ?? 0),
                    IsAuthenticated = true
                };

                // Store in session
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.SetString(SESSION_KEY, System.Text.Json.JsonSerializer.Serialize(userSession));
                }

                _logger.LogInformation("User logged in successfully: {Email}, Role: {Role}", email, userSession.RoleName);
                return userSession;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", email);
                return null;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Session.Remove(SESSION_KEY);
                }
                _logger.LogInformation("User logged out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
            }
        }

        public async Task<UserSession?> GetCurrentUserAsync()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return null;

                var sessionData = httpContext.Session.GetString(SESSION_KEY);
                if (string.IsNullOrEmpty(sessionData)) return null;

                return System.Text.Json.JsonSerializer.Deserialize<UserSession>(sessionData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return null;
            }
        }

        public bool IsAuthenticated()
        {
            var user = GetCurrentUserAsync().Result;
            return user?.IsAuthenticated == true;
        }

        public bool HasRole(int role)
        {
            var user = GetCurrentUserAsync().Result;
            return user?.AccountRole == role;
        }

        public bool IsAdmin()
        {
            var user = GetCurrentUserAsync().Result;
            if (user == null) return false;

            // Check if user is admin (role 3) or if email matches admin config
            var adminConfig = _configuration.GetSection("DefaultAdmin").Get<AdminAccountConfig>();
            return user.AccountRole == 3 || (adminConfig != null && user.AccountEmail == adminConfig.Email);
        }

        public bool IsStaff()
        {
            return HasRole(1);
        }

        public bool IsLecturer()
        {
            return HasRole(2);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string GetRoleName(int role)
        {
            return role switch
            {
                1 => "Staff",
                2 => "Lecturer",
                3 => "Admin",
                _ => "Unknown"
            };
        }

        public bool IsAppSettingsAdmin()
        {
            var user = GetCurrentUserAsync().Result;
            return user?.AccountId == 999; // Special ID for appsettings admin
        }
    }
}
