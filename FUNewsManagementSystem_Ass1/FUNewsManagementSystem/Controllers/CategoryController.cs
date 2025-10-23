using Microsoft.AspNetCore.Mvc;
using Services;
using BusinessObjects;
using Microsoft.AspNetCore.Http;

namespace FUNewsManagementSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly INewsService _newsService;

        public CategoryController(ICategoryService categoryService, INewsService newsService)
        {
            _categoryService = categoryService;
            _newsService = newsService;
        }

        private bool IsStaff()
        {
            var role = HttpContext.Session.GetInt32("Role");
            return role == 1; // 1 = Staff
        }

        public IActionResult Index(string keyword = "")
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var list = _categoryService.Search(keyword);
            ViewBag.Keyword = keyword;
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            ViewBag.Categories = _categoryService.GetAll();
            return PartialView("_CreateOrEdit", new Category());
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _categoryService.GetAll();
                return PartialView("_CreateOrEdit", category);
            }

            _categoryService.Add(category);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var cat = _categoryService.GetById(id);
            ViewBag.Categories = _categoryService.GetAll();
            return PartialView("_CreateOrEdit", cat);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _categoryService.GetAll();
                return PartialView("_CreateOrEdit", category);
            }

            _categoryService.Update(category);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var cat = _categoryService.GetById(id);
            return PartialView("_DeleteConfirm", cat);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            if (_newsService.HasCategory(id))
            {
                return Json(new { success = false, message = "Không thể xóa. Danh mục đã được sử dụng." });
            }

            _categoryService.Delete(id);
            return Json(new { success = true });
        }

        public IActionResult Details(int id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var cat = _categoryService.GetById(id);
            return View(cat);
        }
    }
}
