using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using Microsoft.EntityFrameworkCore;

namespace ASS1.Services
{
    public interface IAdminAccountService
    {
        Task InitializeDefaultAdminAsync();
        Task<bool> AdminAccountExistsAsync();
    }

    public class AdminAccountService : IAdminAccountService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<SystemAccount> _accountRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminAccountService> _logger;

        public AdminAccountService(
            AppDbContext context,
            IGenericRepository<SystemAccount> accountRepository,
            IConfiguration configuration,
            ILogger<AdminAccountService> logger)
        {
            _context = context;
            _accountRepository = accountRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InitializeDefaultAdminAsync()
        {
            try
            {
                // Check if admin account already exists
                if (await AdminAccountExistsAsync())
                {
                    _logger.LogInformation("Default admin account already exists.");
                    return;
                }

                // Get admin configuration from appsettings.json
                var adminConfig = _configuration.GetSection("DefaultAdmin").Get<AdminAccountConfig>();
                if (adminConfig == null)
                {
                    _logger.LogError("DefaultAdmin configuration not found in appsettings.json");
                    return;
                }

                // Create the admin account
                var adminAccount = new SystemAccount
                {
                    AccountName = adminConfig.AccountName,
                    AccountEmail = adminConfig.Email,
                    AccountPassword = adminConfig.Password, // Store password as plain text for simple testing
                    AccountRole = adminConfig.AccountRole
                };

                // Add the admin account to database
                await _accountRepository.AddAsync(adminAccount);
                
                _logger.LogInformation("Default admin account created successfully: {Email}", adminConfig.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default admin account");
                throw;
            }
        }

        public async Task<bool> AdminAccountExistsAsync()
        {
            try
            {
                var adminConfig = _configuration.GetSection("DefaultAdmin").Get<AdminAccountConfig>();
                if (adminConfig == null) return false;

                return await _accountRepository.ExistsAsync(
                    x => x.AccountEmail == adminConfig.Email, 
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if admin account exists");
                return false;
            }
        }

        private string HashPassword(string password)
        {
            // Simple password hashing - in production, use BCrypt or similar
            // For now, we'll use a basic hash for demonstration
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
