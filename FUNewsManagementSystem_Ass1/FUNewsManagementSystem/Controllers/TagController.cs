using Microsoft.AspNetCore.Mvc;
using BusinessObjects;
using Services;
using Microsoft.AspNetCore.Http;

namespace FUNewsManagementSystem.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagService tagService;

        public TagController(ITagService _tagService)
        {
            tagService = _tagService;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            var tags = tagService.GetAllTags();
            return View(tags);
        }

        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Tag tag)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                bool success = tagService.CreateTag(tag, out string error);
                if (success)
                {
                    TempData["Msg"] = "Đã thêm thẻ mới.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            return View(tag);
        }

        public IActionResult Edit(int id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            var tag = tagService.GetTag(id);
            if (tag == null) return NotFound();
            return View(tag);
        }

        [HttpPost]
        public IActionResult Edit(Tag tag)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            if (ModelState.IsValid)
            {
                bool success = tagService.UpdateTag(tag, out string error);
                if (success)
                {
                    TempData["Msg"] = "Đã cập nhật thẻ.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            return View(tag);
        }

        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            bool success = tagService.DeleteTag(id, out string error);
            if (!success)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Msg"] = "Đã xóa thẻ.";
            }
            return RedirectToAction("Index");
        }
    }
}
