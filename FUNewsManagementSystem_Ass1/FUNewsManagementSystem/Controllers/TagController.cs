using Microsoft.AspNetCore.Mvc;
using Services;
using BusinessObjects;
using Microsoft.AspNetCore.Http;

namespace FUNewsManagementSystem.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagService _tagService;
        private readonly INewsTagService _newsTagService;

        public TagController(ITagService tagService, INewsTagService newsTagService)
        {
            _tagService = tagService;
            _newsTagService = newsTagService;
        }

        private bool IsStaff()
        {
            var role = HttpContext.Session.GetInt32("Role");
            return role == 1;
        }

        public IActionResult Index(string keyword = "")
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var list = _tagService.Search(keyword);
            ViewBag.Keyword = keyword;
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");
            return PartialView("_CreateOrEdit", new Tag());
        }

        [HttpPost]
        public IActionResult Create(Tag tag)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid || _tagService.IsDuplicate(tag.TagName))
            {
                ModelState.AddModelError("TagName", "Tên thẻ đã tồn tại.");
                return PartialView("_CreateOrEdit", tag);
            }

            _tagService.Add(tag);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var tag = _tagService.GetById(id);
            return PartialView("_CreateOrEdit", tag);
        }

        [HttpPost]
        public IActionResult Edit(Tag tag)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid || _tagService.IsDuplicate(tag.TagName, tag.TagID))
            {
                ModelState.AddModelError("TagName", "Tên thẻ đã tồn tại.");
                return PartialView("_CreateOrEdit", tag);
            }

            _tagService.Update(tag);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var tag = _tagService.GetById(id);
            return PartialView("_DeleteConfirm", tag);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            if (_newsTagService.HasTag(id))
            {
                return Json(new { success = false, message = "Không thể xóa. Thẻ đang được sử dụng." });
            }

            _tagService.Delete(id);
            return Json(new { success = true });
        }

        public IActionResult Details(int id)
        {
            if (!IsStaff()) return RedirectToAction("Login", "Auth");

            var tag = _tagService.GetById(id);
            ViewBag.UsedInArticles = _newsTagService.GetArticlesByTag(id);
            return View(tag);
        }
    }
}
