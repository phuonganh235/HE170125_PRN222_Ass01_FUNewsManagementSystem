using ASS1.Attributes;
using ASS1.DAL.Models;
using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASS1.Controllers
{
    [RequireStaff]
    public class TagManagementController : Controller
    {
        private readonly ITagManagementService _tagService;
        private readonly ILogger<TagManagementController> _logger;

        public TagManagementController(
            ITagManagementService tagService,
            ILogger<TagManagementController> logger)
        {
            _tagService = tagService;
            _logger = logger;
        }

        // GET: TagManagement
        public async Task<IActionResult> Index(TagSearchViewModel model)
        {
            try
            {
                var result = await _tagService.SearchTagsAsync(
                    model.SearchTerm,
                    model.PageNumber,
                    model.PageSize);

                model.Tags = result.Tags.ToList();
                model.TotalCount = result.TotalCount;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tag management index");
                return View(new TagSearchViewModel());
            }
        }

        // GET: TagManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await _tagService.GetTagDetailsAsync(id);
                if (model.Tag.TagId == 0)
                {
                    return NotFound();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tag details for ID: {Id}", id);
                return NotFound();
            }
        }

        // GET: TagManagement/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TagManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var tag = new Tag
                    {
                        TagName = model.TagName,
                        Note = model.Note
                    };

                    var success = await _tagService.CreateTagAsync(tag);
                    if (success)
                    {
                        _logger.LogInformation("Tag created successfully: {TagName}", model.TagName);
                        TempData["SuccessMessage"] = "Tag created successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to create tag.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating tag");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the tag.");
                }
            }

            return View(model);
        }

        // GET: TagManagement/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                if (tag == null)
                {
                    return NotFound();
                }

                var model = new TagEditViewModel
                {
                    TagId = tag.TagId,
                    TagName = tag.TagName ?? string.Empty,
                    Note = tag.Note
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tag for edit: {Id}", id);
                return NotFound();
            }
        }

        // POST: TagManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TagEditViewModel model)
        {
            if (id != model.TagId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tag = await _tagService.GetTagByIdAsync(id);
                    if (tag == null)
                    {
                        return NotFound();
                    }

                    tag.TagName = model.TagName;
                    tag.Note = model.Note;

                    var success = await _tagService.UpdateTagAsync(tag);
                    if (success)
                    {
                        _logger.LogInformation("Tag updated successfully: {TagName}", model.TagName);
                        TempData["SuccessMessage"] = "Tag updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to update tag.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating tag: {Id}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the tag.");
                }
            }

            return View(model);
        }

        // GET: TagManagement/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var model = await _tagService.GetTagDetailsAsync(id);
                if (model.Tag.TagId == 0)
                {
                    return NotFound();
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading tag for delete: {Id}", id);
                return NotFound();
            }
        }

        // POST: TagManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var canDelete = await _tagService.CanDeleteTagAsync(id);
                if (!canDelete)
                {
                    TempData["ErrorMessage"] = "Cannot delete tag. It is being used by news articles.";
                    return RedirectToAction(nameof(Index));
                }

                var success = await _tagService.DeleteTagAsync(id);
                if (success)
                {
                    _logger.LogInformation("Tag deleted successfully: {Id}", id);
                    TempData["SuccessMessage"] = "Tag deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to delete tag.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tag: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the tag.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: TagManagement/Articles/5
        public async Task<IActionResult> Articles(int id)
        {
            try
            {
                var tag = await _tagService.GetTagByIdAsync(id);
                if (tag == null)
                {
                    return NotFound();
                }

                var articles = await _tagService.GetArticlesByTagAsync(id);
                var articleCount = articles.Count;

                ViewBag.TagName = tag.TagName;
                ViewBag.ArticleCount = articleCount;

                return View(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading articles for tag: {Id}", id);
                return NotFound();
            }
        }
    }
}
