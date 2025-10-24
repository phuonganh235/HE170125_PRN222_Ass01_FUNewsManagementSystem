using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASS1.Services
{
    public interface IAccountManagementService
    {
        Task<(IEnumerable<SystemAccount> Accounts, int TotalCount)> SearchAccountsAsync(
            string? searchTerm, int? roleFilter, int pageNumber = 1, int pageSize = 10);
        Task<SystemAccount?> GetAccountByIdAsync(short accountId);
        Task<bool> CreateAccountAsync(SystemAccount account);
        Task<bool> UpdateAccountAsync(SystemAccount account);
        Task<bool> DeleteAccountAsync(short accountId);
        Task<bool> ChangePasswordAsync(short accountId, string currentPassword, string newPassword);
        Task<bool> IsEmailUniqueAsync(string email, short? excludeAccountId = null);
        Task<bool> CanDeleteAccountAsync(short accountId);
        Task<List<RoleOption>> GetRoleOptionsAsync();
    }

    public class AccountManagementService : IAccountManagementService
    {
        private readonly IGenericRepository<SystemAccount> _accountRepository;
        private readonly IGenericRepository<NewsArticle> _newsRepository;
        private readonly ILogger<AccountManagementService> _logger;

        public AccountManagementService(
            IGenericRepository<SystemAccount> accountRepository,
            IGenericRepository<NewsArticle> newsRepository,
            ILogger<AccountManagementService> logger)
        {
            _accountRepository = accountRepository;
            _newsRepository = newsRepository;
            _logger = logger;
        }

        public async Task<(IEnumerable<SystemAccount> Accounts, int TotalCount)> SearchAccountsAsync(
            string? searchTerm, int? roleFilter, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var filters = new List<Expression<Func<SystemAccount, bool>>>();

                // Apply search term filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    filters.Add(x => (x.AccountName != null && x.AccountName.Contains(searchTerm)) ||
                                   (x.AccountEmail != null && x.AccountEmail.Contains(searchTerm)));
                }

                // Apply role filter
                if (roleFilter.HasValue)
                {
                    filters.Add(x => x.AccountRole == roleFilter.Value);
                }

                var result = await _accountRepository.GetByPageAsync(
                    filters: filters,
                    orderBy: x => x.AccountName ?? string.Empty,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                return (result.Items, result.TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching accounts");
                return (new List<SystemAccount>(), 0);
            }
        }

        public async Task<SystemAccount?> GetAccountByIdAsync(short accountId)
        {
            try
            {
                var accounts = await _accountRepository.GetByPredicateAsync(x => x.AccountId == accountId);
                return accounts.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting account by ID: {AccountId}", accountId);
                return null;
            }
        }

        public async Task<bool> CreateAccountAsync(SystemAccount account)
        {
            try
            {
                // Check if email is unique
                if (!await IsEmailUniqueAsync(account.AccountEmail ?? string.Empty))
                {
                    _logger.LogWarning("Attempted to create account with duplicate email: {Email}", account.AccountEmail);
                    return false;
                }

                // Hash password
                account.AccountPassword = HashPassword(account.AccountPassword ?? string.Empty);

                return await _accountRepository.AddAsync(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating account");
                return false;
            }
        }

        public async Task<bool> UpdateAccountAsync(SystemAccount account)
        {
            try
            {
                // Check if email is unique (excluding current account)
                if (!await IsEmailUniqueAsync(account.AccountEmail ?? string.Empty, account.AccountId))
                {
                    _logger.LogWarning("Attempted to update account with duplicate email: {Email}", account.AccountEmail);
                    return false;
                }

                return await _accountRepository.UpdateAsync(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating account: {AccountId}", account.AccountId);
                return false;
            }
        }

        public async Task<bool> DeleteAccountAsync(short accountId)
        {
            try
            {
                // Check if account can be deleted
                if (!await CanDeleteAccountAsync(accountId))
                {
                    _logger.LogWarning("Cannot delete account {AccountId} - has associated news articles", accountId);
                    return false;
                }

                return await _accountRepository.DeleteAsync(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account: {AccountId}", accountId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(short accountId, string currentPassword, string newPassword)
        {
            try
            {
                var account = await GetAccountByIdAsync(accountId);
                if (account == null)
                {
                    _logger.LogWarning("Account not found for password change: {AccountId}", accountId);
                    return false;
                }

                // Verify current password
                var hashedCurrentPassword = HashPassword(currentPassword);
                if (account.AccountPassword != hashedCurrentPassword)
                {
                    _logger.LogWarning("Invalid current password for account: {AccountId}", accountId);
                    return false;
                }

                // Update password
                account.AccountPassword = HashPassword(newPassword);
                return await _accountRepository.UpdateAsync(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for account: {AccountId}", accountId);
                return false;
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email, short? excludeAccountId = null)
        {
            try
            {
                var accounts = await _accountRepository.GetByPredicateAsync(x => x.AccountEmail == email);
                
                if (excludeAccountId.HasValue)
                {
                    return !accounts.Any(x => x.AccountId != excludeAccountId.Value);
                }
                
                return !accounts.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email uniqueness: {Email}", email);
                return false;
            }
        }

        public async Task<bool> CanDeleteAccountAsync(short accountId)
        {
            try
            {
                var newsArticles = await _newsRepository.GetByPredicateAsync(x => x.CreatedById == accountId);
                return !newsArticles.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if account can be deleted: {AccountId}", accountId);
                return false;
            }
        }

        public async Task<List<RoleOption>> GetRoleOptionsAsync()
        {
            return new List<RoleOption>
            {
                new RoleOption { Value = 1, Text = "Staff" },
                new RoleOption { Value = 2, Text = "Lecturer" },
                new RoleOption { Value = 3, Text = "Admin" }
            };
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
