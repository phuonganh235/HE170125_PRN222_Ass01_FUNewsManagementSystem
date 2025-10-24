using ASS1.Attributes;
using ASS1.DAL.Models;
using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASS1.Controllers
{
    [RequireStaff]
    public class CategoryManagementController : Controller
    {
        private readonly ICategoryManagementService _categoryService;
        private readonly ILogger<CategoryManagementController> _logger;

        public CategoryManagementController(
            ICategoryManagementService categoryService,
            ILogger<CategoryManagementController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: CategoryManagement
        public async Task<IActionResult> Index(CategorySearchViewModel model)
        {
            try
            {
                var result = await _categoryService.SearchCategoriesAsync(
                    model.SearchTerm,
                    model.IsActiveFilter,
                    model.PageNumber,
                    model.PageSize);

                model.Categories = result.Categories.ToList();
                model.TotalCount = result.TotalCount;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category management index");
                return View(new CategorySearchViewModel());
            }
        }

        // GET: CategoryManagement/Details/5
        public async Task<IActionResult> Details(short id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category details for ID: {Id}", id);
                return NotFound();
            }
        }

        // GET: CategoryManagement/Create
        public async Task<IActionResult> Create()
        {
            var model = new CategoryCreateViewModel
            {
                ParentCategoryOptions = await _categoryService.GetParentCategoryOptionsAsync()
            };
            return View(model);
        }

        // POST: CategoryManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var category = new Category
                    {
                        CategoryName = model.CategoryName,
                        CategoryDesciption = model.CategoryDesciption,
                        ParentCategoryId = model.ParentCategoryId == 0 ? null : model.ParentCategoryId,
                        IsActive = model.IsActive
                    };

                    var success = await _categoryService.CreateCategoryAsync(category);
                    if (success)
                    {
                        _logger.LogInformation("Category created successfully: {CategoryName}", model.CategoryName);
                        TempData["SuccessMessage"] = "Category created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create category.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating category");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the category.");
                }
            }

            model.ParentCategoryOptions = await _categoryService.GetParentCategoryOptionsAsync();
            return View(model);
        }

        // GET: CategoryManagement/Edit/5
        public async Task<IActionResult> Edit(short id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                var canChangeParent = await _categoryService.CanChangeParentCategoryAsync(id);
                var model = new CategoryEditViewModel
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName ?? string.Empty,
                    CategoryDesciption = category.CategoryDesciption ?? string.Empty,
                    ParentCategoryId = category.ParentCategoryId,
                    IsActive = category.IsActive ?? false,
                    CanChangeParentCategory = canChangeParent,
                    ParentCategoryOptions = await _categoryService.GetParentCategoryOptionsAsync(id)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category for edit: {Id}", id);
                return NotFound();
            }
        }

        // POST: CategoryManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, CategoryEditViewModel model)
        {
            if (id != model.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _categoryService.GetCategoryByIdAsync(id);
                    if (category == null)
                    {
                        return NotFound();
                    }

                    category.CategoryName = model.CategoryName;
                    category.CategoryDesciption = model.CategoryDesciption;
                    category.IsActive = model.IsActive;

                    // Only update parent category if it can be changed
                    if (model.CanChangeParentCategory)
                    {
                        category.ParentCategoryId = model.ParentCategoryId == 0 ? null : model.ParentCategoryId;
                    }

                    var success = await _categoryService.UpdateCategoryAsync(category);
                    if (success)
                    {
                        _logger.LogInformation("Category updated successfully: {CategoryName}", model.CategoryName);
                        TempData["SuccessMessage"] = "Category updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update category.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating category: {Id}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the category.");
                }
            }

            model.ParentCategoryOptions = await _categoryService.GetParentCategoryOptionsAsync(id);
            return View(model);
        }

        // GET: CategoryManagement/Delete/5
        public async Task<IActionResult> Delete(short id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                var canDelete = await _categoryService.CanDeleteCategoryAsync(id);
                ViewBag.CanDelete = canDelete;

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category for delete: {Id}", id);
                return NotFound();
            }
        }

        // POST: CategoryManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            try
            {
                var canDelete = await _categoryService.CanDeleteCategoryAsync(id);
                if (!canDelete)
                {
                    TempData["ErrorMessage"] = "Cannot delete category. It has associated news articles.";
                    return RedirectToAction(nameof(Index));
                }

                var success = await _categoryService.DeleteCategoryAsync(id);
                if (success)
                {
                    _logger.LogInformation("Category deleted successfully: {Id}", id);
                    TempData["SuccessMessage"] = "Category deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete category.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the category.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: CategoryManagement/ToggleVisibility/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleVisibility(short id)
        {
            try
            {
                var success = await _categoryService.ToggleCategoryVisibilityAsync(id);
                if (success)
                {
                    _logger.LogInformation("Category visibility toggled successfully: {Id}", id);
                    TempData["SuccessMessage"] = "Category visibility updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to update category visibility.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling category visibility: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while updating category visibility.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
