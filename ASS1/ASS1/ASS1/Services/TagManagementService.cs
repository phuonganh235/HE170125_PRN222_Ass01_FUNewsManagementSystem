using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASS1.Services
{
    public interface ITagManagementService
    {
        Task<(IEnumerable<Tag> Tags, int TotalCount)> SearchTagsAsync(
            string? searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<Tag?> GetTagByIdAsync(int tagId);
        Task<bool> CreateTagAsync(Tag tag);
        Task<bool> UpdateTagAsync(Tag tag);
        Task<bool> DeleteTagAsync(int tagId);
        Task<bool> CanDeleteTagAsync(int tagId);
        Task<List<NewsArticle>> GetArticlesByTagAsync(int tagId);
        Task<int> GetArticleCountByTagAsync(int tagId);
        Task<TagDetailsViewModel> GetTagDetailsAsync(int tagId);
    }

    public class TagManagementService : ITagManagementService
    {
        private readonly IGenericRepository<Tag> _tagRepository;
        private readonly IGenericRepository<NewsArticle> _articleRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<TagManagementService> _logger;

        public TagManagementService(
            IGenericRepository<Tag> tagRepository,
            IGenericRepository<NewsArticle> articleRepository,
            AppDbContext context,
            ILogger<TagManagementService> logger)
        {
            _tagRepository = tagRepository;
            _articleRepository = articleRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<(IEnumerable<Tag> Tags, int TotalCount)> SearchTagsAsync(
            string? searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var filters = new List<Expression<Func<Tag, bool>>>();

                // Apply search term filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    filters.Add(x => x.TagName != null && x.TagName.Contains(searchTerm));
                }

                var result = await _tagRepository.GetByPageAsync(
                    filters: filters,
                    orderBy: x => x.TagName ?? string.Empty,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                return (result.Items, result.TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tags");
                return (new List<Tag>(), 0);
            }
        }

        public async Task<Tag?> GetTagByIdAsync(int tagId)
        {
            try
            {
                var tags = await _tagRepository.GetByPredicateAsync(x => x.TagId == tagId);
                return tags.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag by ID: {TagId}", tagId);
                return null;
            }
        }

        public async Task<bool> CreateTagAsync(Tag tag)
        {
            try
            {
                return await _tagRepository.AddAsync(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                return false;
            }
        }

        public async Task<bool> UpdateTagAsync(Tag tag)
        {
            try
            {
                return await _tagRepository.UpdateAsync(tag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag: {TagId}", tag.TagId);
                return false;
            }
        }

        public async Task<bool> DeleteTagAsync(int tagId)
        {
            try
            {
                // Check if tag can be deleted
                if (!await CanDeleteTagAsync(tagId))
                {
                    _logger.LogWarning("Cannot delete tag {TagId} - has associated news articles", tagId);
                    return false;
                }

                return await _tagRepository.DeleteAsync(tagId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag: {TagId}", tagId);
                return false;
            }
        }

        public async Task<bool> CanDeleteTagAsync(int tagId)
        {
            try
            {
                // Check if tag is referenced in NewsTag junction table
                var articlesWithTag = await _context.NewsArticles
                    .Where(x => x.Tags.Any(t => t.TagId == tagId))
                    .AnyAsync();

                return !articlesWithTag;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if tag can be deleted: {TagId}", tagId);
                return false;
            }
        }

        public async Task<List<NewsArticle>> GetArticlesByTagAsync(int tagId)
        {
            try
            {
                var articles = await _context.NewsArticles
                    .Include(x => x.Category)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Tags)
                    .Where(x => x.Tags.Any(t => t.TagId == tagId))
                    .OrderByDescending(x => x.CreatedDate)
                    .ToListAsync();

                return articles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting articles by tag: {TagId}", tagId);
                return new List<NewsArticle>();
            }
        }

        public async Task<int> GetArticleCountByTagAsync(int tagId)
        {
            try
            {
                var count = await _context.NewsArticles
                    .Where(x => x.Tags.Any(t => t.TagId == tagId))
                    .CountAsync();

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article count by tag: {TagId}", tagId);
                return 0;
            }
        }

        public async Task<TagDetailsViewModel> GetTagDetailsAsync(int tagId)
        {
            try
            {
                var tag = await GetTagByIdAsync(tagId);
                if (tag == null)
                {
                    return new TagDetailsViewModel();
                }

                var articles = await GetArticlesByTagAsync(tagId);
                var articleCount = articles.Count;
                var canDelete = await CanDeleteTagAsync(tagId);

                return new TagDetailsViewModel
                {
                    Tag = tag,
                    Articles = articles,
                    ArticleCount = articleCount,
                    CanDelete = canDelete
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag details: {TagId}", tagId);
                return new TagDetailsViewModel();
            }
        }
    }
}
