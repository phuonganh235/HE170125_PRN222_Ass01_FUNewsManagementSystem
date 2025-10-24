using ASS1.Attributes;
using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASS1.Controllers
{
    public class NewsController : Controller
    {
        private readonly IGenericRepository<NewsArticle> _newsRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<Tag> _tagRepository;
        private readonly IAuthenticationService _authService;
        private readonly IRelatedNewsService _relatedNewsService;
        private readonly AppDbContext _context;
        private readonly ILogger<NewsController> _logger;

        public NewsController(
            IGenericRepository<NewsArticle> newsRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<Tag> tagRepository,
            IAuthenticationService authService,
            IRelatedNewsService relatedNewsService,
            AppDbContext context,
            ILogger<NewsController> logger)
        {
            _newsRepository = newsRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _authService = authService;
            _relatedNewsService = relatedNewsService;
            _context = context;
            _logger = logger;
        }

        // Public access - View active news only with optional search
        public async Task<IActionResult> Index(string searchTerm)
        {
            try
            {
                IEnumerable<NewsArticle> activeNews;

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    activeNews = await _newsRepository.GetByPredicateAsync(
                        x => x.NewsStatus == true && 
                             (x.NewsTitle != null && x.NewsTitle.Contains(searchTerm)) ||
                             (x.Headline != null && x.Headline.Contains(searchTerm)) ||
                             (x.NewsContent != null && x.NewsContent.Contains(searchTerm)));
                }
                else
                {
                    activeNews = await _newsRepository.GetByPredicateAsync(
                        x => x.NewsStatus == true);
                }

                ViewBag.SearchTerm = searchTerm;
                return View(activeNews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active news");
                return View(new List<NewsArticle>());
            }
        }

        // Public access - View specific news article
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var news = await _context.NewsArticles
                    .Include(x => x.Category)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Tags)
                    .FirstOrDefaultAsync(x => x.NewsArticleId == id && x.NewsStatus == true);

                if (news == null)
                {
                    return NotFound();
                }

                // Get related articles
                var relatedArticles = await _relatedNewsService.GetRelatedArticlesAsync(id, 3);

                var viewModel = new RelatedNewsViewModel
                {
                    CurrentArticle = news,
                    RelatedArticles = relatedArticles,
                    RelatedCount = relatedArticles.Count
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving news article {Id}", id);
                return NotFound();
            }
        }

        // Staff and Admin access - Create news
        [RequireStaff]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetByPredicateAsync(
                x => x.IsActive == true);
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireStaff]
        public async Task<IActionResult> Create(NewsArticle newsArticle)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser != null)
                    {
                        newsArticle.CreatedById = currentUser.AccountId;
                        newsArticle.CreatedDate = DateTime.Now;
                        newsArticle.NewsStatus = true; // Active by default
                    }

                    await _newsRepository.AddAsync(newsArticle);
                    _logger.LogInformation("News article created: {Id}", newsArticle.NewsArticleId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating news article");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the news article.");
                }
            }

            var categories = await _categoryRepository.GetByPredicateAsync(
                x => x.IsActive == true);
            ViewBag.Categories = categories;
            return View(newsArticle);
        }

        // Staff and Admin access - Edit news
        [RequireStaff]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var news = await _newsRepository.GetByPredicateAsync(x => x.NewsArticleId == id);
                if (!news.Any())
                {
                    return NotFound();
                }

                var categories = await _categoryRepository.GetByPredicateAsync(
                    x => x.IsActive == true);
                ViewBag.Categories = categories;

                return View(news.First());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving news article for edit {Id}", id);
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireStaff]
        public async Task<IActionResult> Edit(string id, NewsArticle newsArticle)
        {
            if (id != newsArticle.NewsArticleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser != null)
                    {
                        newsArticle.UpdatedById = currentUser.AccountId;
                        newsArticle.ModifiedDate = DateTime.Now;
                    }

                    await _newsRepository.UpdateAsync(newsArticle);
                    _logger.LogInformation("News article updated: {Id}", newsArticle.NewsArticleId);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating news article {Id}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the news article.");
                }
            }

            var categories = await _categoryRepository.GetByPredicateAsync(
                x => x.IsActive == true);
            ViewBag.Categories = categories;
            return View(newsArticle);
        }

        // Admin access only - Manage all news
        [RequireAdmin]
        public async Task<IActionResult> Manage()
        {
            try
            {
                var allNews = await _newsRepository.GetAllAsync();
                return View(allNews);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all news for management");
                return View(new List<NewsArticle>());
            }
        }

    }
}
