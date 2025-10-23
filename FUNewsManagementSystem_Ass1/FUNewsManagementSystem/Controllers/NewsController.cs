using Microsoft.AspNetCore.Mvc;
using Services;
using BusinessObjects;
using Microsoft.AspNetCore.Http;

namespace FUNewsManagementSystem.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        private readonly ICategoryService _categoryService;
        private readonly ITagService _tagService;
        private readonly INewsTagService _newsTagService;

        public NewsController(INewsService newsService,
                              ICategoryService categoryService,
                              ITagService tagService,
                              INewsTagService newsTagService)
        {
            _newsService = newsService;
            _categoryService = categoryService;
            _tagService = tagService;
            _newsTagService = newsTagService;
        }

        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        private bool IsStaff()
        {
            var role = HttpContext.Session.GetInt32("Role");
            return role == 1;
        }

        public IActionResult Index(string keyword = "", int? categoryId = null, int? status = null)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var list = _newsService.SearchByStaff(GetUserId(), keyword, categoryId, status);
            ViewBag.Categories = _categoryService.GetAll();
            ViewBag.Keyword = keyword;
            ViewBag.Status = status;
            ViewBag.CategoryId = categoryId;

            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            ViewBag.Categories = _categoryService.GetAll();
            ViewBag.AllTags = _tagService.GetAll();
            return PartialView("_CreateOrEdit", new NewsArticle());
        }

        [HttpPost]
        public IActionResult Create(NewsArticle article, int[] tagIds)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _categoryService.GetAll();
                ViewBag.AllTags = _tagService.GetAll();
                return PartialView("_CreateOrEdit", article);
            }

            article.CreatedById = GetUserId();
            article.CreatedDate = DateTime.Now;
            _newsService.Add(article, tagIds);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var article = _newsService.GetById(id);
            if (article == null) return NotFound();

            ViewBag.Categories = _categoryService.GetAll();
            ViewBag.AllTags = _tagService.GetAll();
            ViewBag.SelectedTags = _newsTagService.GetTagIdsForNews(id);

            return PartialView("_CreateOrEdit", article);
        }

        [HttpPost]
        public IActionResult Edit(NewsArticle article, int[] tagIds)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _categoryService.GetAll();
                ViewBag.AllTags = _tagService.GetAll();
                return PartialView("_CreateOrEdit", article);
            }

            article.UpdatedById = GetUserId();
            article.ModifiedDate = DateTime.Now;
            _newsService.Update(article, tagIds);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var article = _newsService.GetById(id);
            return PartialView("_DeleteConfirm", article);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(string id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            _newsService.Delete(id);
            return Json(new { success = true });
        }

        public IActionResult Details(string id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var article = _newsService.GetById(id);
            return View(article);
        }

        public IActionResult Duplicate(string id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            _newsService.Duplicate(id, GetUserId());
            return RedirectToAction("Index");
        }
    }
}
