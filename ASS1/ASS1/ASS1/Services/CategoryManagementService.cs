using ASS1.DAL.Models;
using ASS1.DAL.Repository;
using ASS1.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ASS1.Services
{
    public interface ICategoryManagementService
    {
        Task<(IEnumerable<Category> Categories, int TotalCount)> SearchCategoriesAsync(
            string? searchTerm, bool? isActiveFilter, int pageNumber = 1, int pageSize = 10);
        Task<Category?> GetCategoryByIdAsync(short categoryId);
        Task<bool> CreateCategoryAsync(Category category);
        Task<bool> UpdateCategoryAsync(Category category);
        Task<bool> DeleteCategoryAsync(short categoryId);
        Task<bool> ToggleCategoryVisibilityAsync(short categoryId);
        Task<bool> CanDeleteCategoryAsync(short categoryId);
        Task<bool> CanChangeParentCategoryAsync(short categoryId);
        Task<List<CategoryOption>> GetParentCategoryOptionsAsync(short? excludeCategoryId = null);
        Task<List<Category>> GetAllActiveCategoriesAsync();
    }

    public class CategoryManagementService : ICategoryManagementService
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<NewsArticle> _newsRepository;
        private readonly ILogger<CategoryManagementService> _logger;

        public CategoryManagementService(
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<NewsArticle> newsRepository,
            ILogger<CategoryManagementService> logger)
        {
            _categoryRepository = categoryRepository;
            _newsRepository = newsRepository;
            _logger = logger;
        }

        public async Task<(IEnumerable<Category> Categories, int TotalCount)> SearchCategoriesAsync(
            string? searchTerm, bool? isActiveFilter, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                var filters = new List<Expression<Func<Category, bool>>>();

                // Apply search term filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    filters.Add(x => (x.CategoryName != null && x.CategoryName.Contains(searchTerm)) ||
                                   (x.CategoryDesciption != null && x.CategoryDesciption.Contains(searchTerm)));
                }

                // Apply active filter
                if (isActiveFilter.HasValue)
                {
                    filters.Add(x => x.IsActive == isActiveFilter.Value);
                }

                var result = await _categoryRepository.GetByPageAsync(
                    filters: filters,
                    orderBy: x => x.CategoryName ?? string.Empty,
                    pageNumber: pageNumber,
                    pageSize: pageSize);

                return (result.Items, result.TotalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching categories");
                return (new List<Category>(), 0);
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(short categoryId)
        {
            try
            {
                var categories = await _categoryRepository.GetByPredicateAsync(x => x.CategoryId == categoryId);
                return categories.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category by ID: {CategoryId}", categoryId);
                return null;
            }
        }

        public async Task<bool> CreateCategoryAsync(Category category)
        {
            try
            {
                return await _categoryRepository.AddAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                return await _categoryRepository.UpdateAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category: {CategoryId}", category.CategoryId);
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(short categoryId)
        {
            try
            {
                // Check if category can be deleted
                if (!await CanDeleteCategoryAsync(categoryId))
                {
                    _logger.LogWarning("Cannot delete category {CategoryId} - has associated news articles", categoryId);
                    return false;
                }

                return await _categoryRepository.DeleteAsync(categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {CategoryId}", categoryId);
                return false;
            }
        }

        public async Task<bool> ToggleCategoryVisibilityAsync(short categoryId)
        {
            try
            {
                var category = await GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    _logger.LogWarning("Category not found for visibility toggle: {CategoryId}", categoryId);
                    return false;
                }

                category.IsActive = !category.IsActive;
                return await _categoryRepository.UpdateAsync(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category visibility: {CategoryId}", categoryId);
                return false;
            }
        }

        public async Task<bool> CanDeleteCategoryAsync(short categoryId)
        {
            try
            {
                var newsArticles = await _newsRepository.GetByPredicateAsync(x => x.CategoryId == categoryId);
                return !newsArticles.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if category can be deleted: {CategoryId}", categoryId);
                return false;
            }
        }

        public async Task<bool> CanChangeParentCategoryAsync(short categoryId)
        {
            try
            {
                var newsArticles = await _newsRepository.GetByPredicateAsync(x => x.CategoryId == categoryId);
                return !newsArticles.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if parent category can be changed: {CategoryId}", categoryId);
                return false;
            }
        }

        public async Task<List<CategoryOption>> GetParentCategoryOptionsAsync(short? excludeCategoryId = null)
        {
            try
            {
                var categories = await _categoryRepository.GetByPredicateAsync(x => x.IsActive == true);
                
                if (excludeCategoryId.HasValue)
                {
                    categories = categories.Where(x => x.CategoryId != excludeCategoryId.Value);
                }

                var options = new List<CategoryOption>();
                
                // Add root level option
                options.Add(new CategoryOption { Value = 0, Text = "No Parent (Root Category)", Level = 0 });

                // Add categories with hierarchy
                var rootCategories = categories.Where(x => x.ParentCategoryId == null).OrderBy(x => x.CategoryName);
                foreach (var category in rootCategories)
                {
                    options.Add(new CategoryOption { Value = category.CategoryId, Text = category.CategoryName ?? string.Empty, Level = 0 });
                    AddChildCategories(categories, category.CategoryId, options, 1);
                }

                return options;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parent category options");
                return new List<CategoryOption>();
            }
        }

        public async Task<List<Category>> GetAllActiveCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetByPredicateAsync(x => x.IsActive == true);
                return categories.OrderBy(x => x.CategoryName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all active categories");
                return new List<Category>();
            }
        }

        private void AddChildCategories(IEnumerable<Category> allCategories, short parentId, List<CategoryOption> options, int level)
        {
            var children = allCategories.Where(x => x.ParentCategoryId == parentId).OrderBy(x => x.CategoryName);
            foreach (var child in children)
            {
                var indent = new string('─', level * 2);
                options.Add(new CategoryOption 
                { 
                    Value = child.CategoryId, 
                    Text = $"{indent} {child.CategoryName}", 
                    Level = level 
                });
                AddChildCategories(allCategories, child.CategoryId, options, level + 1);
            }
        }
    }
}
