using ASS1.Attributes;
using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASS1.Controllers
{
    [RequireStaff]
    public class NewsTagLinkController : Controller
    {
        private readonly INewsTagLinkService _newsTagService;
        private readonly ILogger<NewsTagLinkController> _logger;

        public NewsTagLinkController(
            INewsTagLinkService newsTagService,
            ILogger<NewsTagLinkController> logger)
        {
            _newsTagService = newsTagService;
            _logger = logger;
        }

        // GET: NewsTagLink/Manage/{articleId}
        public async Task<IActionResult> Manage(string articleId)
        {
            try
            {
                var model = await _newsTagService.GetArticleTagManagementAsync(articleId);
                if (string.IsNullOrEmpty(model.NewsArticleId))
                {
                    return NotFound();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tag management for article: {ArticleId}", articleId);
                return NotFound();
            }
        }

        // POST: NewsTagLink/AddTag
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTag(TagAssignmentViewModel model)
        {
            try
            {
                var success = await _newsTagService.AddTagToArticleAsync(model.NewsArticleId, model.TagId);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Tag '{model.TagName}' added successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add tag to article.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tag to article: {ArticleId}, {TagId}", model.NewsArticleId, model.TagId);
                TempData["ErrorMessage"] = "An error occurred while adding the tag.";
            }

            return RedirectToAction("Manage", new { articleId = model.NewsArticleId });
        }

        // POST: NewsTagLink/RemoveTag
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTag(TagAssignmentViewModel model)
        {
            try
            {
                var success = await _newsTagService.RemoveTagFromArticleAsync(model.NewsArticleId, model.TagId);
                if (success)
                {
                    TempData["SuccessMessage"] = $"Tag '{model.TagName}' removed successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to remove tag from article.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tag from article: {ArticleId}, {TagId}", model.NewsArticleId, model.TagId);
                TempData["ErrorMessage"] = "An error occurred while removing the tag.";
            }

            return RedirectToAction("Manage", new { articleId = model.NewsArticleId });
        }

        // POST: NewsTagLink/UpdateTags
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTags(ArticleTagManagementViewModel model)
        {
            try
            {
                var success = await _newsTagService.UpdateArticleTagsAsync(model.NewsArticleId, model.SelectedTagIds);
                if (success)
                {
                    TempData["SuccessMessage"] = "Article tags updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update article tags.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tags for article: {ArticleId}", model.NewsArticleId);
                TempData["ErrorMessage"] = "An error occurred while updating the tags.";
            }

            return RedirectToAction("Manage", new { articleId = model.NewsArticleId });
        }

        // GET: NewsTagLink/GetTags/{articleId}
        [HttpGet]
        public async Task<IActionResult> GetTags(string articleId)
        {
            try
            {
                var tags = await _newsTagService.GetTagsForArticleAsync(articleId);
                return Json(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags for article: {ArticleId}", articleId);
                return Json(new List<NewsTagLinkViewModel>());
            }
        }

        // GET: NewsTagLink/GetAvailableTags/{articleId}
        [HttpGet]
        public async Task<IActionResult> GetAvailableTags(string articleId)
        {
            try
            {
                var tags = await _newsTagService.GetAvailableTagsForArticleAsync(articleId);
                return Json(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tags for article: {ArticleId}", articleId);
                return Json(new List<TagOption>());
            }
        }
    }
}
