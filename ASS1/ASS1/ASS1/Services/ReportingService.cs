using ASS1.DAL.Models;
using ASS1.Models;
using Microsoft.EntityFrameworkCore;

namespace ASS1.Services
{
    public interface IReportingService
    {
        Task<ReportViewModel> GenerateReportAsync(DateTime? fromDate, DateTime? toDate);
        Task<List<CategoryReportItem>> GetCategoryReportAsync(DateTime? fromDate, DateTime? toDate);
        Task<List<AuthorReportItem>> GetAuthorReportAsync(DateTime? fromDate, DateTime? toDate);
        Task<List<StatusReportItem>> GetStatusReportAsync(DateTime? fromDate, DateTime? toDate);
        Task<ReportSummary> GetReportSummaryAsync(DateTime? fromDate, DateTime? toDate);
    }

    public class ReportingService : IReportingService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportingService> _logger;

        public ReportingService(AppDbContext context, ILogger<ReportingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ReportViewModel> GenerateReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var categoryReports = await GetCategoryReportAsync(fromDate, toDate);
                var authorReports = await GetAuthorReportAsync(fromDate, toDate);
                var statusReports = await GetStatusReportAsync(fromDate, toDate);
                var summary = await GetReportSummaryAsync(fromDate, toDate);

                return new ReportViewModel
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    CategoryReports = categoryReports,
                    AuthorReports = authorReports,
                    StatusReports = statusReports,
                    Summary = summary
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                return new ReportViewModel();
            }
        }

        public async Task<List<CategoryReportItem>> GetCategoryReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = _context.NewsArticles
                    .Include(x => x.Category)
                    .AsQueryable();

                // Apply date filter
                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate <= toDate.Value);
                }

                var categoryGroups = await query
                    .GroupBy(x => new { x.CategoryId, CategoryName = x.Category != null ? x.Category.CategoryName : "No Category" })
                    .Select(g => new CategoryReportItem
                    {
                        CategoryId = g.Key.CategoryId ?? 0,
                        CategoryName = g.Key.CategoryName ?? "No Category",
                        ArticleCount = g.Count(),
                        ActiveCount = g.Count(x => x.NewsStatus == true),
                        InactiveCount = g.Count(x => x.NewsStatus == false),
                        LatestArticleDate = g.Max(x => x.CreatedDate),
                        OldestArticleDate = g.Min(x => x.CreatedDate)
                    })
                    .OrderByDescending(x => x.ArticleCount)
                    .ToListAsync();

                return categoryGroups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating category report");
                return new List<CategoryReportItem>();
            }
        }

        public async Task<List<AuthorReportItem>> GetAuthorReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = _context.NewsArticles
                    .Include(x => x.CreatedBy)
                    .AsQueryable();

                // Apply date filter
                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate <= toDate.Value);
                }

                var authorGroups = await query
                    .Where(x => x.CreatedBy != null)
                    .GroupBy(x => new { x.CreatedById, AuthorName = x.CreatedBy!.AccountName, AuthorEmail = x.CreatedBy!.AccountEmail })
                    .Select(g => new AuthorReportItem
                    {
                        AuthorId = g.Key.CreatedById ?? 0,
                        AuthorName = g.Key.AuthorName ?? "Unknown",
                        AuthorEmail = g.Key.AuthorEmail ?? "Unknown",
                        ArticleCount = g.Count(),
                        ActiveCount = g.Count(x => x.NewsStatus == true),
                        InactiveCount = g.Count(x => x.NewsStatus == false),
                        LatestArticleDate = g.Max(x => x.CreatedDate),
                        OldestArticleDate = g.Min(x => x.CreatedDate)
                    })
                    .OrderByDescending(x => x.ArticleCount)
                    .ToListAsync();

                return authorGroups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating author report");
                return new List<AuthorReportItem>();
            }
        }

        public async Task<List<StatusReportItem>> GetStatusReportAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = _context.NewsArticles.AsQueryable();

                // Apply date filter
                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate <= toDate.Value);
                }

                var statusGroups = await query
                    .GroupBy(x => x.NewsStatus)
                    .Select(g => new StatusReportItem
                    {
                        Status = g.Key ?? false,
                        StatusName = g.Key == true ? "Active" : "Inactive",
                        ArticleCount = g.Count(),
                        LatestArticleDate = g.Max(x => x.CreatedDate),
                        OldestArticleDate = g.Min(x => x.CreatedDate)
                    })
                    .OrderByDescending(x => x.ArticleCount)
                    .ToListAsync();

                // Calculate percentages
                var totalArticles = statusGroups.Sum(x => x.ArticleCount);
                foreach (var status in statusGroups)
                {
                    status.Percentage = totalArticles > 0 ? (double)status.ArticleCount / totalArticles * 100 : 0;
                }

                return statusGroups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating status report");
                return new List<StatusReportItem>();
            }
        }

        public async Task<ReportSummary> GetReportSummaryAsync(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var query = _context.NewsArticles.AsQueryable();

                // Apply date filter
                if (fromDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(x => x.CreatedDate <= toDate.Value);
                }

                var totalArticles = await query.CountAsync();
                var activeArticles = await query.CountAsync(x => x.NewsStatus == true);
                var inactiveArticles = await query.CountAsync(x => x.NewsStatus == false);

                var uniqueCategories = await query
                    .Where(x => x.CategoryId != null)
                    .Select(x => x.CategoryId)
                    .Distinct()
                    .CountAsync();

                var uniqueAuthors = await query
                    .Where(x => x.CreatedById != null)
                    .Select(x => x.CreatedById)
                    .Distinct()
                    .CountAsync();

                var activePercentage = totalArticles > 0 ? (double)activeArticles / totalArticles * 100 : 0;
                var inactivePercentage = totalArticles > 0 ? (double)inactiveArticles / totalArticles * 100 : 0;

                return new ReportSummary
                {
                    TotalArticles = totalArticles,
                    TotalActiveArticles = activeArticles,
                    TotalInactiveArticles = inactiveArticles,
                    ActivePercentage = Math.Round(activePercentage, 2),
                    InactivePercentage = Math.Round(inactivePercentage, 2),
                    TotalCategories = uniqueCategories,
                    TotalAuthors = uniqueAuthors,
                    ReportGeneratedDate = DateTime.Now,
                    DateRangeStart = fromDate,
                    DateRangeEnd = toDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report summary");
                return new ReportSummary();
            }
        }
    }
}
