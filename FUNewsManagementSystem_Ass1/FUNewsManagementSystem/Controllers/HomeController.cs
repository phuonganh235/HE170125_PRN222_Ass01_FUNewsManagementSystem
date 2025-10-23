using Microsoft.AspNetCore.Mvc;
using Services;
using BusinessObjects;
using System.Linq;
using System.Collections.Generic;

namespace FUNewsManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly INewsService newsService;
        private readonly ICategoryService categoryService;

        public HomeController(INewsService _newsService, ICategoryService _categoryService)
        {
            newsService = _newsService;
            categoryService = _categoryService;
        }

        // Trang chủ: danh sách các bài đã xuất bản
        public IActionResult Index()
        {
            // Lấy tất cả bài viết Published
            var publishedNews = newsService.GetAllNews("Published");
            // Có thể muốn chỉ lấy một số mới nhất, ví dụ:
            publishedNews = publishedNews.OrderByDescending(n => n.CreatedDate).ToList();
            return View(publishedNews);
        }

        // Trang chi tiết bài viết
        public IActionResult Details(int id)
        {
            var article = newsService.GetNews(id);
            if (article == null || article.NewsStatus.ToLower() != "published")
            {
                return NotFound(); // Không tồn tại hoặc chưa xuất bản thì 404
            }
            // Lấy danh sách bài liên quan: cùng Category, khác id hiện tại
            List<NewsArticle> related = new List<NewsArticle>();
            if (article.CategoryId != 0)
            {
                var sameCategoryNews = newsService.GetAllNews().Where(n => n.CategoryId == article.CategoryId
                                                                            && n.NewsArticleId != id
                                                                            && n.NewsStatus.ToLower() == "published");
                // Lấy 3 bài mới nhất trong cùng danh mục
                related = sameCategoryNews.OrderByDescending(n => n.CreatedDate).Take(3).ToList();
            }
            ViewBag.RelatedArticles = related;
            return View(article);
        }

        // (Tùy chọn) Trang lọc bài viết theo danh mục
        public IActionResult Category(int id)
        {
            // Lấy danh mục
            var category = categoryService.GetCategory(id);
            if (category == null) return NotFound();
            // Lấy các bài viết đã xuất bản thuộc danh mục đó
            var list = newsService.GetAllNews("Published").Where(n => n.CategoryId == id);
            ViewBag.Category = category;
            return View(list);
        }
    }
}
