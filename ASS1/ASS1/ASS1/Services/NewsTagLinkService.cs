using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using Microsoft.EntityFrameworkCore;

namespace ASS1.Services
{
    public interface INewsTagLinkService
    {
        Task<List<NewsTagLinkViewModel>> GetTagsForArticleAsync(string newsArticleId);
        Task<bool> AddTagToArticleAsync(string newsArticleId, int tagId);
        Task<bool> RemoveTagFromArticleAsync(string newsArticleId, int tagId);
        Task<bool> UpdateArticleTagsAsync(string newsArticleId, List<int> tagIds);
        Task<List<TagOption>> GetAvailableTagsForArticleAsync(string newsArticleId);
        Task<ArticleTagManagementViewModel> GetArticleTagManagementAsync(string newsArticleId);
        Task<bool> TagExistsForArticleAsync(string newsArticleId, int tagId);
        Task<int> GetTagCountForArticleAsync(string newsArticleId);
    }

    public class NewsTagLinkService : INewsTagLinkService
    {
        private readonly IGenericRepository<NewsArticle> _articleRepository;
        private readonly IGenericRepository<Tag> _tagRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<NewsTagLinkService> _logger;

        public NewsTagLinkService(
            IGenericRepository<NewsArticle> articleRepository,
            IGenericRepository<Tag> tagRepository,
            AppDbContext context,
            ILogger<NewsTagLinkService> logger)
        {
            _articleRepository = articleRepository;
            _tagRepository = tagRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<List<NewsTagLinkViewModel>> GetTagsForArticleAsync(string newsArticleId)
        {
            try
            {
                var article = await _context.NewsArticles
                    .Include(x => x.Tags)
                    .FirstOrDefaultAsync(x => x.NewsArticleId == newsArticleId);

                if (article == null)
                {
                    return new List<NewsTagLinkViewModel>();
                }

                return article.Tags.Select(tag => new NewsTagLinkViewModel
                {
                    NewsArticleId = newsArticleId,
                    TagId = tag.TagId,
                    TagName = tag.TagName ?? string.Empty,
                    ArticleTitle = article.NewsTitle ?? string.Empty
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tags for article: {NewsArticleId}", newsArticleId);
                return new List<NewsTagLinkViewModel>();
            }
        }

        public async Task<bool> AddTagToArticleAsync(string newsArticleId, int tagId)
        {
            try
            {
                // Check if the relationship already exists (primary key constraint)
                if (await TagExistsForArticleAsync(newsArticleId, tagId))
                {
                    _logger.LogWarning("Tag {TagId} already exists for article {NewsArticleId}", tagId, newsArticleId);
                    return true; // Already exists, consider it successful
                }

                var article = await _context.NewsArticles
                    .Include(x => x.Tags)
                    .FirstOrDefaultAsync(x => x.NewsArticleId == newsArticleId);

                var tag = await _tagRepository.GetByPredicateAsync(x => x.TagId == tagId);
                var tagEntity = tag.FirstOrDefault();

                if (article == null || tagEntity == null)
                {
                    _logger.LogWarning("Article {NewsArticleId} or Tag {TagId} not found", newsArticleId, tagId);
                    return false;
                }

                article.Tags.Add(tagEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Tag {TagId} added to article {NewsArticleId}", tagId, newsArticleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tag {TagId} to article {NewsArticleId}", tagId, newsArticleId);
                return false;
            }
        }

        public async Task<bool> RemoveTagFromArticleAsync(string newsArticleId, int tagId)
        {
            try
            {
                var article = await _context.NewsArticles
                    .Include(x => x.Tags)
                    .FirstOrDefaultAsync(x => x.NewsArticleId == newsArticleId);

                if (article == null)
                {
                    _logger.LogWarning("Article {NewsArticleId} not found", newsArticleId);
                    return false;
                }

                var tagToRemove = article.Tags.FirstOrDefault(t => t.TagId == tagId);
                if (tagToRemove == null)
                {
                    _logger.LogWarning("Tag {TagId} not found for article {NewsArticleId}", tagId, newsArticleId);
                    return false;
                }

                article.Tags.Remove(tagToRemove);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Tag {TagId} removed from article {NewsArticleId}", tagId, newsArticleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing tag {TagId} from article {NewsArticleId}", tagId, newsArticleId);
                return false;
            }
        }

        public async Task<bool> UpdateArticleTagsAsync(string newsArticleId, List<int> tagIds)
        {
            try
            {
                var article = await _context.NewsArticles
                    .Include(x => x.Tags)
                    .FirstOrDefaultAsync(x => x.NewsArticleId == newsArticleId);

                if (article == null)
                {
                    _logger.LogWarning("Article {NewsArticleId} not found", newsArticleId);
                    return false;
                }

                // Clear existing tags
                article.Tags.Clear();

                // Add new tags
                if (tagIds.Any())
                {
                    var tags = await _tagRepository.GetByPredicateAsync(x => tagIds.Contains(x.TagId));
                    foreach (var tag in tags)
                    {
                        article.Tags.Add(tag);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Tags updated for article {NewsArticleId}: {TagCount} tags", newsArticleId, tagIds.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tags for article {NewsArticleId}", newsArticleId);
                return false;
            }
        }

        public async Task<List<TagOption>> GetAvailableTagsForArticleAsync(string newsArticleId)
        {
            try
            {
                var allTags = await _tagRepository.GetAllAsync();
                var currentTagIds = await GetTagsForArticleAsync(newsArticleId);
                var currentTagIdSet = currentTagIds.Select(t => t.TagId).ToHashSet();

                return allTags
                    .Where(t => !currentTagIdSet.Contains(t.TagId))
                    .OrderBy(t => t.TagName)
                    .Select(t => new TagOption { Value = t.TagId, Text = t.TagName ?? string.Empty })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available tags for article {NewsArticleId}", newsArticleId);
                return new List<TagOption>();
            }
        }

        public async Task<ArticleTagManagementViewModel> GetArticleTagManagementAsync(string newsArticleId)
        {
            try
            {
                var article = await _articleRepository.GetByPredicateAsync(x => x.NewsArticleId == newsArticleId);
                var articleEntity = article.FirstOrDefault();

                if (articleEntity == null)
                {
                    return new ArticleTagManagementViewModel();
                }

                var currentTags = await GetTagsForArticleAsync(newsArticleId);
                var availableTags = await GetAvailableTagsForArticleAsync(newsArticleId);

                return new ArticleTagManagementViewModel
                {
                    NewsArticleId = newsArticleId,
                    ArticleTitle = articleEntity.NewsTitle ?? string.Empty,
                    CurrentTags = currentTags,
                    AvailableTags = availableTags,
                    SelectedTagIds = currentTags.Select(t => t.TagId).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article tag management for {NewsArticleId}", newsArticleId);
                return new ArticleTagManagementViewModel();
            }
        }

        public async Task<bool> TagExistsForArticleAsync(string newsArticleId, int tagId)
        {
            try
            {
                var exists = await _context.NewsArticles
                    .Where(x => x.NewsArticleId == newsArticleId)
                    .AnyAsync(x => x.Tags.Any(t => t.TagId == tagId));

                return exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if tag exists for article: {NewsArticleId}, {TagId}", newsArticleId, tagId);
                return false;
            }
        }

        public async Task<int> GetTagCountForArticleAsync(string newsArticleId)
        {
            try
            {
                var count = await _context.NewsArticles
                    .Where(x => x.NewsArticleId == newsArticleId)
                    .SelectMany(x => x.Tags)
                    .CountAsync();

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag count for article: {NewsArticleId}", newsArticleId);
                return 0;
            }
        }
    }
}
