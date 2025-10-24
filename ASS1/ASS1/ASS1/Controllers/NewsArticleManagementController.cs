using ASS1.Attributes;
using ASS1.DAL.Models;
using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASS1.Controllers
{
    [RequireStaff]
    public class NewsArticleManagementController : Controller
    {
        private readonly INewsArticleManagementService _newsService;
        private readonly IAuthenticationService _authService;
        private readonly ILogger<NewsArticleManagementController> _logger;

        public NewsArticleManagementController(
            INewsArticleManagementService newsService,
            IAuthenticationService authService,
            ILogger<NewsArticleManagementController> logger)
        {
            _newsService = newsService;
            _authService = authService;
            _logger = logger;
        }

        // GET: NewsArticleManagement
        public async Task<IActionResult> Index(NewsArticleSearchViewModel model)
        {
            try
            {
                var result = await _newsService.SearchArticlesAsync(model);
                model.Articles = result.Articles.ToList();
                model.TotalCount = result.TotalCount;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading news article management index");
                return View(new NewsArticleSearchViewModel());
            }
        }

        // GET: NewsArticleManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var article = await _newsService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                return View(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading article details for ID: {Id}", id);
                return NotFound();
            }
        }

        // GET: NewsArticleManagement/Create
        public async Task<IActionResult> Create()
        {
            var model = new NewsArticleCreateViewModel
            {
                CategoryOptions = await _newsService.GetCategoryOptionsAsync(),
                TagOptions = await _newsService.GetTagOptionsAsync()
            };
            return View(model);
        }

        // POST: NewsArticleManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsArticleCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    // Generate unique article ID
                    var articleId = GenerateArticleId();

                    var article = new NewsArticle
                    {
                        NewsArticleId = articleId,
                        NewsTitle = model.NewsTitle,
                        Headline = model.Headline,
                        NewsContent = model.NewsContent,
                        NewsSource = model.NewsSource,
                        CategoryId = model.CategoryId,
                        NewsStatus = model.NewsStatus
                    };

                    var success = await _newsService.CreateArticleAsync(article, model.SelectedTagIds, currentUser.AccountId);
                    if (success)
                    {
                        _logger.LogInformation("Article created successfully: {ArticleId}", articleId);
                        TempData["SuccessMessage"] = "Article created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create article.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating article");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the article.");
                }
            }

            model.CategoryOptions = await _newsService.GetCategoryOptionsAsync();
            model.TagOptions = await _newsService.GetTagOptionsAsync();
            return View(model);
        }

        // GET: NewsArticleManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var article = await _newsService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                var model = new NewsArticleEditViewModel
                {
                    NewsArticleId = article.NewsArticleId,
                    NewsTitle = article.NewsTitle,
                    Headline = article.Headline,
                    NewsContent = article.NewsContent,
                    NewsSource = article.NewsSource,
                    CategoryId = article.CategoryId,
                    NewsStatus = article.NewsStatus,
                    SelectedTagIds = article.Tags.Select(t => t.TagId).ToList(),
                    CurrentTags = article.Tags.ToList(),
                    CategoryOptions = await _newsService.GetCategoryOptionsAsync(),
                    TagOptions = await _newsService.GetTagOptionsAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading article for edit: {Id}", id);
                return NotFound();
            }
        }

        // POST: NewsArticleManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, NewsArticleEditViewModel model)
        {
            if (id != model.NewsArticleId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    var article = await _newsService.GetArticleByIdAsync(id);
                    if (article == null)
                    {
                        return NotFound();
                    }

                    article.NewsTitle = model.NewsTitle;
                    article.Headline = model.Headline;
                    article.NewsContent = model.NewsContent;
                    article.NewsSource = model.NewsSource;
                    article.CategoryId = model.CategoryId;
                    article.NewsStatus = model.NewsStatus ?? false;

                    var success = await _newsService.UpdateArticleAsync(article, model.SelectedTagIds, currentUser.AccountId);
                    if (success)
                    {
                        _logger.LogInformation("Article updated successfully: {ArticleId}", id);
                        TempData["SuccessMessage"] = "Article updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update article.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating article: {Id}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the article.");
                }
            }

            model.CategoryOptions = await _newsService.GetCategoryOptionsAsync();
            model.TagOptions = await _newsService.GetTagOptionsAsync();
            return View(model);
        }

        // GET: NewsArticleManagement/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var article = await _newsService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                return View(article);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading article for delete: {Id}", id);
                return NotFound();
            }
        }

        // POST: NewsArticleManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                var success = await _newsService.DeleteArticleAsync(id);
                if (success)
                {
                    _logger.LogInformation("Article deleted successfully: {Id}", id);
                    TempData["SuccessMessage"] = "Article deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete article.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the article.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: NewsArticleManagement/Duplicate/5
        public async Task<IActionResult> Duplicate(string id)
        {
            try
            {
                var article = await _newsService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                var model = new NewsArticleDuplicateViewModel
                {
                    OriginalNewsArticleId = id,
                    NewNewsArticleId = GenerateArticleId(),
                    NewsTitle = article.NewsTitle,
                    Headline = article.Headline,
                    NewsContent = article.NewsContent,
                    NewsSource = article.NewsSource,
                    CategoryId = article.CategoryId,
                    NewsStatus = article.NewsStatus ?? false,
                    SelectedTagIds = article.Tags.Select(t => t.TagId).ToList(),
                    CategoryOptions = await _newsService.GetCategoryOptionsAsync(),
                    TagOptions = await _newsService.GetTagOptionsAsync()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading article for duplicate: {Id}", id);
                return NotFound();
            }
        }

        // POST: NewsArticleManagement/Duplicate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(NewsArticleDuplicateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _authService.GetCurrentUserAsync();
                    if (currentUser == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    // Check if new article ID is unique
                    if (!await _newsService.IsArticleIdUniqueAsync(model.NewNewsArticleId))
                    {
                        ModelState.AddModelError("NewNewsArticleId", "This article ID already exists. Please choose a different one.");
                    }
                    else
                    {
                        var duplicatedArticle = await _newsService.DuplicateArticleAsync(
                            model.OriginalNewsArticleId, 
                            model.NewNewsArticleId, 
                            currentUser.AccountId);

                        if (duplicatedArticle != null)
                        {
                            // Update the duplicated article with new values
                            duplicatedArticle.NewsTitle = model.NewsTitle;
                            duplicatedArticle.Headline = model.Headline;
                            duplicatedArticle.NewsContent = model.NewsContent;
                            duplicatedArticle.NewsSource = model.NewsSource;
                            duplicatedArticle.CategoryId = model.CategoryId;
                            duplicatedArticle.NewsStatus = model.NewsStatus;

                            var success = await _newsService.UpdateArticleAsync(duplicatedArticle, model.SelectedTagIds, currentUser.AccountId);
                            if (success)
                            {
                                _logger.LogInformation("Article duplicated successfully: {OriginalId} to {NewId}", 
                                    model.OriginalNewsArticleId, model.NewNewsArticleId);
                                TempData["SuccessMessage"] = "Article duplicated successfully.";
                                return RedirectToAction(nameof(Index));
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Failed to duplicate article.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Failed to duplicate article.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error duplicating article");
                    ModelState.AddModelError(string.Empty, "An error occurred while duplicating the article.");
                }
            }

            model.CategoryOptions = await _newsService.GetCategoryOptionsAsync();
            model.TagOptions = await _newsService.GetTagOptionsAsync();
            return View(model);
        }

        private string GenerateArticleId()
        {
            return $"NEWS_{DateTime.Now:yyyyMMdd}_{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}
