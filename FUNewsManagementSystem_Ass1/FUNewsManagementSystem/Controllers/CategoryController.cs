using Microsoft.AspNetCore.Mvc;
using BusinessObjects;
using Services;
using Microsoft.AspNetCore.Http;

namespace FUNewsManagementSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService categoryService;

        public CategoryController(ICategoryService _categoryService)
        {
            categoryService = _categoryService;
        }

        // Danh sách danh mục
        public IActionResult Index()
        {
            // Yêu cầu đăng nhập (Admin hoặc Staff đều có thể nếu chính sách cho phép,
            // giả sử cả Admin và Staff đều xem được danh mục)
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            var categories = categoryService.GetAllCategories();
            return View(categories);
        }

        // Form tạo mới danh mục
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            // Có thể tải danh sách Category cha để chọn (loại trừ danh mục con)
            ViewBag.ParentCategories = categoryService.GetAllCategories().Where(c => c.IsActive);
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                bool success = categoryService.CreateCategory(category, out string error);
                if (success)
                {
                    TempData["Msg"] = "Đã tạo danh mục mới.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            // Nạp lại danh sách ParentCategory cho dropdown nếu có lỗi
            ViewBag.ParentCategories = categoryService.GetAllCategories().Where(c => c.IsActive);
            return View(category);
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            var category = categoryService.GetCategory(id);
            if (category == null) return NotFound();
            ViewBag.ParentCategories = categoryService.GetAllCategories().Where(c => c.IsActive && c.CategoryId != id);
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                bool success = categoryService.UpdateCategory(category, out string error);
                if (success)
                {
                    TempData["Msg"] = "Đã cập nhật danh mục.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            // Nạp lại ParentCategories, tránh trường hợp dropdown bị trống do Postback
            ViewBag.ParentCategories = categoryService.GetAllCategories().Where(c => c.IsActive && c.CategoryId != category.CategoryId);
            return View(category);
        }

        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            bool success = categoryService.DeleteCategory(id, out string error);
            if (!success)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Msg"] = "Đã xóa danh mục.";
            }
            return RedirectToAction("Index");
        }
    }
}
