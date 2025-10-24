using ASS1.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace ASS1.Services
{
    public interface IRelatedNewsService
    {
        Task<List<NewsArticle>> GetRelatedArticlesAsync(string currentNewsArticleId, int maxCount = 3);
        Task<List<NewsArticle>> GetRelatedByCategoryAsync(string currentNewsArticleId, short? categoryId, int maxCount = 3);
        Task<List<NewsArticle>> GetRelatedByTagsAsync(string currentNewsArticleId, int maxCount = 3);
    }

    public class RelatedNewsService : IRelatedNewsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RelatedNewsService> _logger;

        public RelatedNewsService(AppDbContext context, ILogger<RelatedNewsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<NewsArticle>> GetRelatedArticlesAsync(string currentNewsArticleId, int maxCount = 3)
        {
            try
            {
                // Get the current article to find its category and tags
                var currentArticle = await _context.NewsArticles
                    .Include(x => x.Category)
                    .Include(x => x.Tags)
                    .FirstOrDefaultAsync(x => x.NewsArticleId == currentNewsArticleId);

                if (currentArticle == null)
                {
                    return new List<NewsArticle>();
                }

                var relatedArticles = new List<NewsArticle>();

                // Get articles from the same category
                var categoryArticles = await GetRelatedByCategoryAsync(currentNewsArticleId, currentArticle.CategoryId, maxCount);
                relatedArticles.AddRange(categoryArticles);

                // If we don't have enough articles, get more from shared tags
                if (relatedArticles.Count < maxCount)
                {
                    var remainingCount = maxCount - relatedArticles.Count;
                    var tagArticles = await GetRelatedByTagsAsync(currentNewsArticleId, remainingCount);
                    
                    // Filter out articles already included from category
                    var existingIds = relatedArticles.Select(x => x.NewsArticleId).ToHashSet();
                    var newTagArticles = tagArticles.Where(x => !existingIds.Contains(x.NewsArticleId)).ToList();
                    
                    relatedArticles.AddRange(newTagArticles);
                }

                // Remove duplicates and limit to maxCount
                return relatedArticles
                    .GroupBy(x => x.NewsArticleId)
                    .Select(g => g.First())
                    .Take(maxCount)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related articles for {NewsArticleId}", currentNewsArticleId);
                return new List<NewsArticle>();
            }
        }

        public async Task<List<NewsArticle>> GetRelatedByCategoryAsync(string currentNewsArticleId, short? categoryId, int maxCount = 3)
        {
            try
            {
                if (!categoryId.HasValue)
                {
                    return new List<NewsArticle>();
                }

                var articles = await _context.NewsArticles
                    .Include(x => x.Category)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Tags)
                    .Where(x => x.NewsArticleId != currentNewsArticleId &&
                               x.CategoryId == categoryId &&
                               x.NewsStatus == true)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(maxCount)
                    .ToListAsync();

                return articles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related articles by category for {NewsArticleId}", currentNewsArticleId);
                return new List<NewsArticle>();
            }
        }

        public async Task<List<NewsArticle>> GetRelatedByTagsAsync(string currentNewsArticleId, int maxCount = 3)
        {
            try
            {
                // Get current article's tags
                var currentArticleTags = await _context.NewsArticles
                    .Where(x => x.NewsArticleId == currentNewsArticleId)
                    .SelectMany(x => x.Tags)
                    .Select(x => x.TagId)
                    .ToListAsync();

                if (!currentArticleTags.Any())
                {
                    return new List<NewsArticle>();
                }

                // Find articles that share at least one tag with the current article
                var relatedArticleIds = await _context.NewsArticles
                    .Where(x => x.NewsArticleId != currentNewsArticleId &&
                               x.NewsStatus == true &&
                               x.Tags.Any(t => currentArticleTags.Contains(t.TagId)))
                    .Select(x => x.NewsArticleId)
                    .ToListAsync();

                // Get the full article details
                var articles = await _context.NewsArticles
                    .Include(x => x.Category)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Tags)
                    .Where(x => relatedArticleIds.Contains(x.NewsArticleId))
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(maxCount)
                    .ToListAsync();

                return articles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related articles by tags for {NewsArticleId}", currentNewsArticleId);
                return new List<NewsArticle>();
            }
        }
    }
}
