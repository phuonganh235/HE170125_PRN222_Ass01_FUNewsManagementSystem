using ASS1.Attributes;
using ASS1.DAL.Models;
using ASS1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASS1.Controllers
{
    [RequireAdmin]
    public class AuditController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AuditController> _logger;

        public AuditController(AppDbContext context, ILogger<AuditController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Audit
        public async Task<IActionResult> Index()
        {
            try
            {
                var articlesWithAudit = await _context.NewsArticles
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Category)
                    .Where(x => x.ModifiedDate.HasValue) // Only show articles that have been modified
                    .OrderByDescending(x => x.ModifiedDate)
                    .ToListAsync();

                // Get all unique UpdatedById values to fetch the editors
                var updatedByIds = articlesWithAudit
                    .Where(x => x.UpdatedById.HasValue)
                    .Select(x => x.UpdatedById.Value)
                    .Distinct()
                    .ToList();

                var editors = await _context.SystemAccounts
                    .Where(x => updatedByIds.Contains(x.AccountId))
                    .ToDictionaryAsync(x => x.AccountId, x => x);

                var auditViewModels = articlesWithAudit.Select(article => new NewsArticleWithAuditViewModel
                {
                    Article = article,
                    LastEditor = article.UpdatedById.HasValue && editors.ContainsKey(article.UpdatedById.Value) 
                        ? editors[article.UpdatedById.Value] 
                        : null,
                    Creator = article.CreatedBy
                }).ToList();

                return View(auditViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading audit index");
                return View(new List<NewsArticleWithAuditViewModel>());
            }
        }

        // GET: Audit/Details/5
        public async Task<IActionResult> Details(string id)
        {
            try
            {
                var article = await _context.NewsArticles
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Category)
                    .Include(x => x.Tags)
                    .FirstOrDefaultAsync(x => x.NewsArticleId == id);

                if (article == null)
                {
                    return NotFound();
                }

                // Get the editor if UpdatedById exists
                SystemAccount? lastEditor = null;
                if (article.UpdatedById.HasValue)
                {
                    lastEditor = await _context.SystemAccounts
                        .FirstOrDefaultAsync(x => x.AccountId == article.UpdatedById.Value);
                }

                var auditViewModel = new NewsArticleWithAuditViewModel
                {
                    Article = article,
                    LastEditor = lastEditor,
                    Creator = article.CreatedBy
                };

                return View(auditViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading audit details for article: {Id}", id);
                return NotFound();
            }
        }

        // GET: Audit/RecentChanges
        public async Task<IActionResult> RecentChanges(int days = 7)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-days);
                
                var recentChanges = await _context.NewsArticles
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Category)
                    .Where(x => x.ModifiedDate >= cutoffDate)
                    .OrderByDescending(x => x.ModifiedDate)
                    .ToListAsync();

                // Get all unique UpdatedById values to fetch the editors
                var updatedByIds = recentChanges
                    .Where(x => x.UpdatedById.HasValue)
                    .Select(x => x.UpdatedById.Value)
                    .Distinct()
                    .ToList();

                var editors = await _context.SystemAccounts
                    .Where(x => updatedByIds.Contains(x.AccountId))
                    .ToDictionaryAsync(x => x.AccountId, x => x);

                var auditViewModels = recentChanges.Select(article => new NewsArticleWithAuditViewModel
                {
                    Article = article,
                    LastEditor = article.UpdatedById.HasValue && editors.ContainsKey(article.UpdatedById.Value) 
                        ? editors[article.UpdatedById.Value] 
                        : null,
                    Creator = article.CreatedBy
                }).ToList();

                ViewBag.Days = days;
                return View(auditViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading recent changes for {Days} days", days);
                return View(new List<NewsArticleWithAuditViewModel>());
            }
        }

        // GET: Audit/ByEditor
        public async Task<IActionResult> ByEditor(short? editorId = null)
        {
            try
            {
                var query = _context.NewsArticles
                    .Include(x => x.CreatedBy)
                    .Include(x => x.Category)
                    .Where(x => x.ModifiedDate.HasValue);

                if (editorId.HasValue)
                {
                    query = query.Where(x => x.UpdatedById == editorId);
                }

                var articlesWithAudit = await query
                    .OrderByDescending(x => x.ModifiedDate)
                    .ToListAsync();

                // Get all unique UpdatedById values to fetch the editors
                var updatedByIds = articlesWithAudit
                    .Where(x => x.UpdatedById.HasValue)
                    .Select(x => x.UpdatedById.Value)
                    .Distinct()
                    .ToList();

                var editorsDict = await _context.SystemAccounts
                    .Where(x => updatedByIds.Contains(x.AccountId))
                    .ToDictionaryAsync(x => x.AccountId, x => x);

                var auditViewModels = articlesWithAudit.Select(article => new NewsArticleWithAuditViewModel
                {
                    Article = article,
                    LastEditor = article.UpdatedById.HasValue && editorsDict.ContainsKey(article.UpdatedById.Value) 
                        ? editorsDict[article.UpdatedById.Value] 
                        : null,
                    Creator = article.CreatedBy
                }).ToList();

                // Get list of editors for dropdown
                var editors = await _context.SystemAccounts
                    .Where(x => x.AccountRole == 1 || x.AccountRole == 3) // Staff and Admin
                    .OrderBy(x => x.AccountName)
                    .ToListAsync();

                ViewBag.Editors = editors;
                ViewBag.SelectedEditorId = editorId;

                return View(auditViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading audit by editor");
                return View(new List<NewsArticleWithAuditViewModel>());
            }
        }
    }
}
