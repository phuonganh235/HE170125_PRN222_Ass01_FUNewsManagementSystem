using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASS1.Services
{
    public interface INewsArticleManagementService
    {
        Task<(IEnumerable<NewsArticle> Articles, int TotalCount)> SearchArticlesAsync(
            NewsArticleSearchViewModel searchModel);
        Task<NewsArticle?> GetArticleByIdAsync(string articleId);
        Task<bool> CreateArticleAsync(NewsArticle article, List<int> tagIds, short createdById);
        Task<bool> UpdateArticleAsync(NewsArticle article, List<int> tagIds, short updatedById);
        Task<bool> DeleteArticleAsync(string articleId);
        Task<NewsArticle?> DuplicateArticleAsync(string originalId, string newId, short createdById);
        Task<List<CategoryOption>> GetCategoryOptionsAsync();
        Task<List<TagOption>> GetTagOptionsAsync();
        Task<bool> IsArticleIdUniqueAsync(string articleId);
    }

    public class NewsArticleManagementService : INewsArticleManagementService
    {
        private readonly IGenericRepository<NewsArticle> _articleRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<Tag> _tagRepository;
        private readonly IGenericRepository<SystemAccount> _accountRepository;
        private readonly AppDbContext _context;
        private readonly ILogger<NewsArticleManagementService> _logger;

        public NewsArticleManagementService(
            IGenericRepository<NewsArticle> articleRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<Tag> tagRepository,
            IGenericRepository<SystemAccount> accountRepository,
            AppDbContext context,
            ILogger<NewsArticleManagementService> logger)
        {
            _articleRepository = articleRepository;
            _categoryRepository = categoryRepository;
            _tagRepository = tagRepository;
            _accountRepository = accountRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<(IEnumerable<NewsArticle> Articles, int TotalCount)> SearchArticlesAsync(
            NewsArticleSearchViewModel searchModel)
        {
            try
            {
                var query = _context.NewsArticles
                    .Include(x => x.Category)
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Tags)
                    .AsQueryable();

                // Apply search filters
                if (!string.IsNullOrWhiteSpace(searchModel.SearchTerm))
                {
                    query = query.Where(x => (x.NewsTitle != null && x.NewsTitle.Contains(searchModel.SearchTerm)) ||
                                           (x.Headline != null && x.Headline.Contains(searchModel.SearchTerm)) ||
                                           (x.NewsContent != null && x.NewsContent.Contains(searchModel.SearchTerm)));
                }

                if (!string.IsNullOrWhiteSpace(searchModel.AuthorName))
                {
                    query = query.Where(x => x.CreatedBy != null && 
                                           x.CreatedBy.AccountName != null && 
                                           x.CreatedBy.AccountName.Contains(searchModel.AuthorName));
                }

                if (!string.IsNullOrWhiteSpace(searchModel.CategoryName))
                {
                    query = query.Where(x => x.Category != null && 
                                           x.Category.CategoryName != null && 
                                           x.Category.CategoryName.Contains(searchModel.CategoryName));
                }

                if (searchModel.StatusFilter.HasValue)
                {
                    query = query.Where(x => x.NewsStatus == searchModel.StatusFilter.Value);
                }

                if (searchModel.CreatedDateFrom.HasValue)
                {
                    query = query.Where(x => x.CreatedDate >= searchModel.CreatedDateFrom.Value);
                }

                if (searchModel.CreatedDateTo.HasValue)
                {
                    query = query.Where(x => x.CreatedDate <= searchModel.CreatedDateTo.Value);
                }

                // Apply sorting
                query = searchModel.SortBy.ToLower() switch
                {
                    "title" => searchModel.SortDescending ? query.OrderByDescending(x => x.NewsTitle) : query.OrderBy(x => x.NewsTitle),
                    "headline" => searchModel.SortDescending ? query.OrderByDescending(x => x.Headline) : query.OrderBy(x => x.Headline),
                    "category" => searchModel.SortDescending ? query.OrderByDescending(x => x.Category!.CategoryName) : query.OrderBy(x => x.Category!.CategoryName),
                    "author" => searchModel.SortDescending ? query.OrderByDescending(x => x.CreatedBy!.AccountName) : query.OrderBy(x => x.CreatedBy!.AccountName),
                    "status" => searchModel.SortDescending ? query.OrderByDescending(x => x.NewsStatus) : query.OrderBy(x => x.NewsStatus),
                    _ => searchModel.SortDescending ? query.OrderByDescending(x => x.CreatedDate) : query.OrderBy(x => x.CreatedDate)
                };

                var totalCount = await query.CountAsync();
                var articles = await query
                    .Skip((searchModel.PageNumber - 1) * searchModel.PageSize)
                    .Take(searchModel.PageSize)
                    .ToListAsync();

                return (articles, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching articles");
                return (new List<NewsArticle>(), 0);
            }
        }

        public async Task<NewsArticle?> GetArticleByIdAsync(string articleId)
        {
            try
            {
                var articles = await _articleRepository.GetByPredicateAsync(x => x.NewsArticleId == articleId);
                return articles.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting article by ID: {ArticleId}", articleId);
                return null;
            }
        }

        public async Task<bool> CreateArticleAsync(NewsArticle article, List<int> tagIds, short createdById)
        {
            try
            {
                article.CreatedById = createdById;
                article.CreatedDate = DateTime.Now;

                var success = await _articleRepository.AddAsync(article);
                if (success && tagIds.Any())
                {
                    await UpdateArticleTags(article.NewsArticleId, tagIds);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating article");
                return false;
            }
        }

        public async Task<bool> UpdateArticleAsync(NewsArticle article, List<int> tagIds, short updatedById)
        {
            try
            {
                article.UpdatedById = updatedById;
                article.ModifiedDate = DateTime.Now;

                var success = await _articleRepository.UpdateAsync(article);
                if (success)
                {
                    await UpdateArticleTags(article.NewsArticleId, tagIds);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article: {ArticleId}", article.NewsArticleId);
                return false;
            }
        }

        public async Task<bool> DeleteArticleAsync(string articleId)
        {
            try
            {
                // Remove related NewsTag records first
                await RemoveArticleTags(articleId);

                // Delete the article
                return await _articleRepository.DeleteAsync(articleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article: {ArticleId}", articleId);
                return false;
            }
        }

        public async Task<NewsArticle?> DuplicateArticleAsync(string originalId, string newId, short createdById)
        {
            try
            {
                var originalArticle = await GetArticleByIdAsync(originalId);
                if (originalArticle == null)
                {
                    return null;
                }

                // Check if new ID is unique
                if (!await IsArticleIdUniqueAsync(newId))
                {
                    return null;
                }

                var duplicatedArticle = new NewsArticle
                {
                    NewsArticleId = newId,
                    NewsTitle = originalArticle.NewsTitle,
                    Headline = originalArticle.Headline,
                    NewsContent = originalArticle.NewsContent,
                    NewsSource = originalArticle.NewsSource,
                    CategoryId = originalArticle.CategoryId,
                    NewsStatus = originalArticle.NewsStatus,
                    CreatedById = createdById,
                    CreatedDate = DateTime.Now
                };

                var success = await _articleRepository.AddAsync(duplicatedArticle);
                if (success && originalArticle.Tags.Any())
                {
                    var tagIds = originalArticle.Tags.Select(t => t.TagId).ToList();
                    await UpdateArticleTags(newId, tagIds);
                }

                return success ? duplicatedArticle : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating article: {OriginalId} to {NewId}", originalId, newId);
                return null;
            }
        }

        public async Task<List<CategoryOption>> GetCategoryOptionsAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetByPredicateAsync(x => x.IsActive == true);
                return categories
                    .OrderBy(x => x.CategoryName)
                    .Select(x => new CategoryOption { Value = x.CategoryId, Text = x.CategoryName ?? string.Empty })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category options");
                return new List<CategoryOption>();
            }
        }

        public async Task<List<TagOption>> GetTagOptionsAsync()
        {
            try
            {
                var tags = await _tagRepository.GetAllAsync();
                return tags
                    .OrderBy(x => x.TagName)
                    .Select(x => new TagOption { Value = x.TagId, Text = x.TagName ?? string.Empty })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tag options");
                return new List<TagOption>();
            }
        }

        public async Task<bool> IsArticleIdUniqueAsync(string articleId)
        {
            try
            {
                var articles = await _articleRepository.GetByPredicateAsync(x => x.NewsArticleId == articleId);
                return !articles.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking article ID uniqueness: {ArticleId}", articleId);
                return false;
            }
        }

        private async Task UpdateArticleTags(string articleId, List<int> tagIds)
        {
            try
            {
                // Remove existing tags
                await RemoveArticleTags(articleId);

                // Add new tags
                if (tagIds.Any())
                {
                    var article = await GetArticleByIdAsync(articleId);
                    if (article != null)
                    {
                        var tags = await _tagRepository.GetByPredicateAsync(x => tagIds.Contains(x.TagId));
                        foreach (var tag in tags)
                        {
                            article.Tags.Add(tag);
                        }
                        await _articleRepository.UpdateAsync(article);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article tags for article: {ArticleId}", articleId);
            }
        }

        private async Task RemoveArticleTags(string articleId)
        {
            try
            {
                var article = await GetArticleByIdAsync(articleId);
                if (article != null)
                {
                    article.Tags.Clear();
                    await _articleRepository.UpdateAsync(article);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing article tags for article: {ArticleId}", articleId);
            }
        }
    }
}
