using ASS1.Attributes;
using ASS1.DAL.Models;
using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASS1.Controllers
{
    [RequireAdmin]
    public class AccountManagementController : Controller
    {
        private readonly IAccountManagementService _accountService;
        private readonly ILogger<AccountManagementController> _logger;

        public AccountManagementController(
            IAccountManagementService accountService,
            ILogger<AccountManagementController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        // GET: AccountManagement
        public async Task<IActionResult> Index(AccountSearchViewModel model)
        {
            try
            {
                var result = await _accountService.SearchAccountsAsync(
                    model.SearchTerm,
                    model.RoleFilter,
                    model.PageNumber,
                    model.PageSize);

                model.Accounts = result.Accounts.ToList();
                model.TotalCount = result.TotalCount;
                model.RoleOptions = await _accountService.GetRoleOptionsAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading account management index");
                return View(new AccountSearchViewModel { RoleOptions = await _accountService.GetRoleOptionsAsync() });
            }
        }

        // GET: AccountManagement/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    return NotFound();
                }

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading account details for ID: {Id}", id);
                return NotFound();
            }
        }

        // GET: AccountManagement/Create
        public async Task<IActionResult> Create()
        {
            var model = new AccountCreateViewModel
            {
                RoleOptions = await _accountService.GetRoleOptionsAsync()
            };
            return View(model);
        }

        // POST: AccountManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var account = new SystemAccount
                    {
                        AccountName = model.AccountName,
                        AccountEmail = model.AccountEmail,
                        AccountPassword = model.AccountPassword,
                        AccountRole = model.AccountRole
                    };

                    var success = await _accountService.CreateAccountAsync(account);
                    if (success)
                    {
                        _logger.LogInformation("Account created successfully: {Email}", model.AccountEmail);
                        TempData["SuccessMessage"] = "Account created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create account. Email may already exist.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating account");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the account.");
                }
            }

            model.RoleOptions = await _accountService.GetRoleOptionsAsync();
            return View(model);
        }

        // GET: AccountManagement/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    return NotFound();
                }

                var model = new AccountEditViewModel
                {
                    AccountId = account.AccountId,
                    AccountName = account.AccountName ?? string.Empty,
                    AccountEmail = account.AccountEmail ?? string.Empty,
                    AccountRole = account.AccountRole ?? 0,
                    RoleOptions = await _accountService.GetRoleOptionsAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading account for edit: {Id}", id);
                return NotFound();
            }
        }

        // POST: AccountManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, AccountEditViewModel model)
        {
            if (id != model.AccountId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var account = await _accountService.GetAccountByIdAsync(id);
                    if (account == null)
                    {
                        return NotFound();
                    }

                    account.AccountName = model.AccountName;
                    account.AccountEmail = model.AccountEmail;
                    account.AccountRole = model.AccountRole;

                    var success = await _accountService.UpdateAccountAsync(account);
                    if (success)
                    {
                        _logger.LogInformation("Account updated successfully: {Email}", model.AccountEmail);
                        TempData["SuccessMessage"] = "Account updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update account. Email may already exist.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating account: {Id}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the account.");
                }
            }

            model.RoleOptions = await _accountService.GetRoleOptionsAsync();
            return View(model);
        }

        // GET: AccountManagement/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    return NotFound();
                }

                var canDelete = await _accountService.CanDeleteAccountAsync(id);
                ViewBag.CanDelete = canDelete;

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading account for delete: {Id}", id);
                return NotFound();
            }
        }

        // POST: AccountManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var canDelete = await _accountService.CanDeleteAccountAsync(id);
                if (!canDelete)
                {
                    TempData["ErrorMessage"] = "Cannot delete account. It has associated news articles.";
                    return RedirectToAction(nameof(Index));
                }

                var success = await _accountService.DeleteAccountAsync(id);
                if (success)
                {
                    _logger.LogInformation("Account deleted successfully: {Id}", id);
                    TempData["SuccessMessage"] = "Account deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete account.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the account.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: AccountManagement/ChangePassword/5
        public async Task<IActionResult> ChangePassword(short id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    return NotFound();
                }

                var model = new PasswordChangeViewModel
                {
                    AccountId = account.AccountId
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading account for password change: {Id}", id);
                return NotFound();
            }
        }

        // POST: AccountManagement/ChangePassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(PasswordChangeViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _accountService.ChangePasswordAsync(
                        model.AccountId,
                        model.CurrentPassword,
                        model.NewPassword);

                    if (success)
                    {
                        _logger.LogInformation("Password changed successfully for account: {Id}", model.AccountId);
                        TempData["SuccessMessage"] = "Password changed successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to change password. Current password may be incorrect.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error changing password for account: {Id}", model.AccountId);
                    ModelState.AddModelError(string.Empty, "An error occurred while changing the password.");
                }
            }

            return View(model);
        }
    }
}
