using Microsoft.AspNetCore.Mvc;
using BusinessObjects;
using Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace FUNewsManagementSystem.Controllers
{
    public class NewsController : Controller
    {
        private readonly INewsService newsService;
        private readonly ICategoryService categoryService;
        private readonly ITagService tagService;

        public NewsController(INewsService _newsService, ICategoryService _categoryService, ITagService _tagService)
        {
            newsService = _newsService;
            categoryService = _categoryService;
            tagService = _tagService;
        }

        // Danh sách bài viết (có thể thêm filter theo status hoặc category)
        public IActionResult Index(string status)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            IEnumerable<NewsArticle> newsList;
            if (!string.IsNullOrEmpty(status))
            {
                newsList = newsService.GetAllNews(status);
            }
            else
            {
                newsList = newsService.GetAllNews();
            }
            return View(newsList);
        }

        // Form tạo bài viết
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            // Lấy danh sách Category & Tag cho view
            ViewBag.Categories = categoryService.GetAllCategories().Where(c => c.IsActive);
            ViewBag.Tags = tagService.GetAllTags();
            return View();
        }

        [HttpPost]
        public IActionResult Create(NewsArticle news, List<int> tagIds)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            // Gán thông tin người tạo
            news.CreatedById = HttpContext.Session.GetInt32("UserId") ?? 0;
            news.UpdatedById = null;
            if (ModelState.IsValid)
            {
                bool success = newsService.CreateNews(news, tagIds, out string error);
                if (success)
                {
                    TempData["Msg"] = "Đã tạo bài viết mới.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            // Nếu có lỗi, cần nạp lại danh sách Category, Tag để hiển thị lại form
            ViewBag.Categories = categoryService.GetAllCategories().Where(c => c.IsActive);
            ViewBag.Tags = tagService.GetAllTags();
            return View(news);
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            var article = newsService.GetNews(id);
            if (article == null) return NotFound();
            ViewBag.Categories = categoryService.GetAllCategories().Where(c => c.IsActive);
            ViewBag.Tags = tagService.GetAllTags();
            // Lấy danh sách tagId của bài viết để gửi sang view (đánh dấu checkbox)
            var currentTags = new List<int>();
            foreach (var nt in article.NewsTags)
            {
                currentTags.Add(nt.TagId);
            }
            ViewBag.CurrentTagIds = currentTags;
            return View(article);
        }

        [HttpPost]
        public IActionResult Edit(NewsArticle news, List<int> tagIds)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            // Giữ CreatedById & CreatedDate cũ
            // Khi model binding, nếu NewsArticle có CreatedDate không được gửi từ form, nó sẽ là default -> cần giữ thủ công
            var existing = newsService.GetNews(news.NewsArticleId);
            if (existing == null) return NotFound();
            news.CreatedById = existing.CreatedById;
            news.CreatedDate = existing.CreatedDate;
            // Gán người cập nhật
            news.UpdatedById = HttpContext.Session.GetInt32("UserId");
            if (ModelState.IsValid)
            {
                bool success = newsService.UpdateNews(news, tagIds, out string error);
                if (success)
                {
                    TempData["Msg"] = "Đã cập nhật bài viết.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            // Nạp lại dropdown và checklist
            ViewBag.Categories = categoryService.GetAllCategories().Where(c => c.IsActive);
            ViewBag.Tags = tagService.GetAllTags();
            ViewBag.CurrentTagIds = tagIds;
            return View(news);
        }

        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            bool success = newsService.DeleteNews(id, out string error);
            if (!success)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Msg"] = "Đã xóa bài viết.";
            }
            return RedirectToAction("Index");
        }
    }
}
