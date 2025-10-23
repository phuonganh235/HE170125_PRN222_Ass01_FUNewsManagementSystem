using Microsoft.AspNetCore.Mvc;
using BusinessObjects;
using Services;
using Microsoft.AspNetCore.Http;

namespace FUNewsManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService accountService;

        public AccountController(IAccountService _accountService)
        {
            accountService = _accountService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì chuyển hướng (ví dụ: Admin -> Account/Index, Staff -> News/Index)
            if (HttpContext.Session.GetString("UserRole") != null)
            {
                string role = HttpContext.Session.GetString("UserRole");
                if (role == "Admin") return RedirectToAction("Index", "Account");
                else return RedirectToAction("Index", "News");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ email và mật khẩu.";
                return View();
            }
            var account = accountService.Login(email, password);
            if (account != null)
            {
                // Lưu thông tin user vào Session
                HttpContext.Session.SetInt32("UserId", account.AccountId);
                HttpContext.Session.SetString("UserName", account.AccountName);
                HttpContext.Session.SetString("UserRole", account.AccountRole);
                // Điều hướng theo role
                if (account.AccountRole == "Admin")
                {
                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    return RedirectToAction("Index", "News");
                }
            }
            else
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng!";
                return View();
            }
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            // Xóa session
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: /Account/Index (Danh sách tài khoản) - chỉ Admin
        public IActionResult Index()
        {
            // Kiểm tra role
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                return RedirectToAction("Login");
            }
            var accounts = accountService.GetAccounts();
            return View(accounts);
        }

        // GET: /Account/Create - (Admin)
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login");
            return View();
        }

        // POST: /Account/Create
        [HttpPost]
        public IActionResult Create(SystemAccount account)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login");
            if (ModelState.IsValid)
            {
                bool success = accountService.CreateAccount(account, out string error);
                if (success)
                {
                    TempData["Msg"] = "Tạo tài khoản mới thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            return View(account);
        }

        // GET: /Account/Edit/5
        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login");
            var acc = accountService.GetAccount(id);
            if (acc == null) return NotFound();
            return View(acc);
        }

        // POST: /Account/Edit/5
        [HttpPost]
        public IActionResult Edit(SystemAccount account)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login");
            if (ModelState.IsValid)
            {
                bool success = accountService.UpdateAccount(account, out string error);
                if (success)
                {
                    TempData["Msg"] = "Cập nhật tài khoản thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            return View(account);
        }

        // GET: /Account/Delete/5
        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Login");
            var acc = accountService.GetAccount(id);
            if (acc == null) return NotFound();
            // Không xóa chính mình (nếu muốn ngăn admin tự xóa bản thân)
            if (acc.AccountId == HttpContext.Session.GetInt32("UserId"))
            {
                TempData["Msg"] = "Không thể tự xóa tài khoản của bạn!";
                return RedirectToAction("Index");
            }
            // Gọi service xóa
            bool success = accountService.DeleteAccount(id, out string error);
            if (!success)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Msg"] = "Đã xóa tài khoản.";
            }
            return RedirectToAction("Index");
        }
    }
}
