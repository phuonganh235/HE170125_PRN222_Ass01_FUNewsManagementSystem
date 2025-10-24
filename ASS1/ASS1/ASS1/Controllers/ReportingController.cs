using ASS1.Attributes;
using ASS1.Models;
using ASS1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASS1.Controllers
{
    [RequireAdmin]
    public class ReportingController : Controller
    {
        private readonly IReportingService _reportingService;
        private readonly ILogger<ReportingController> _logger;

        public ReportingController(
            IReportingService reportingService,
            ILogger<ReportingController> logger)
        {
            _reportingService = reportingService;
            _logger = logger;
        }

        // GET: Reporting
        public async Task<IActionResult> Index(ReportViewModel model)
        {
            try
            {
                var report = await _reportingService.GenerateReportAsync(model.FromDate, model.ToDate);
                
                // Preserve filter values
                report.FromDate = model.FromDate;
                report.ToDate = model.ToDate;

                return View(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reporting index");
                return View(new ReportViewModel());
            }
        }

        // GET: Reporting/CategoryReport
        public async Task<IActionResult> CategoryReport(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var categoryReports = await _reportingService.GetCategoryReportAsync(fromDate, toDate);
                return View(categoryReports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category report");
                return View(new List<CategoryReportItem>());
            }
        }

        // GET: Reporting/AuthorReport
        public async Task<IActionResult> AuthorReport(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var authorReports = await _reportingService.GetAuthorReportAsync(fromDate, toDate);
                return View(authorReports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading author report");
                return View(new List<AuthorReportItem>());
            }
        }

        // GET: Reporting/StatusReport
        public async Task<IActionResult> StatusReport(DateTime? fromDate, DateTime? toDate)
        {
            try
            {
                var statusReports = await _reportingService.GetStatusReportAsync(fromDate, toDate);
                return View(statusReports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading status report");
                return View(new List<StatusReportItem>());
            }
        }

        // GET: Reporting/Export
        public async Task<IActionResult> Export(DateTime? fromDate, DateTime? toDate, string format = "csv")
        {
            try
            {
                var report = await _reportingService.GenerateReportAsync(fromDate, toDate);
                
                if (format.ToLower() == "csv")
                {
                    return await ExportToCsv(report);
                }
                else if (format.ToLower() == "json")
                {
                    return await ExportToJson(report);
                }
                else
                {
                    return BadRequest("Unsupported export format");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                return BadRequest("Error exporting report");
            }
        }

        private async Task<IActionResult> ExportToCsv(ReportViewModel report)
        {
            var csv = new System.Text.StringBuilder();
            
            // Summary
            csv.AppendLine("Report Summary");
            csv.AppendLine($"Total Articles,{report.Summary.TotalArticles}");
            csv.AppendLine($"Active Articles,{report.Summary.TotalActiveArticles}");
            csv.AppendLine($"Inactive Articles,{report.Summary.TotalInactiveArticles}");
            csv.AppendLine($"Active Percentage,{report.Summary.ActivePercentage}%");
            csv.AppendLine($"Inactive Percentage,{report.Summary.InactivePercentage}%");
            csv.AppendLine();

            // Category Report
            csv.AppendLine("Category Report");
            csv.AppendLine("Category ID,Category Name,Article Count,Active Count,Inactive Count,Latest Article Date,Oldest Article Date");
            foreach (var item in report.CategoryReports)
            {
                csv.AppendLine($"{item.CategoryId},{item.CategoryName},{item.ArticleCount},{item.ActiveCount},{item.InactiveCount},{item.LatestArticleDate:yyyy-MM-dd},{item.OldestArticleDate:yyyy-MM-dd}");
            }
            csv.AppendLine();

            // Author Report
            csv.AppendLine("Author Report");
            csv.AppendLine("Author ID,Author Name,Author Email,Article Count,Active Count,Inactive Count,Latest Article Date,Oldest Article Date");
            foreach (var item in report.AuthorReports)
            {
                csv.AppendLine($"{item.AuthorId},{item.AuthorName},{item.AuthorEmail},{item.ArticleCount},{item.ActiveCount},{item.InactiveCount},{item.LatestArticleDate:yyyy-MM-dd},{item.OldestArticleDate:yyyy-MM-dd}");
            }
            csv.AppendLine();

            // Status Report
            csv.AppendLine("Status Report");
            csv.AppendLine("Status,Status Name,Article Count,Percentage,Latest Article Date,Oldest Article Date");
            foreach (var item in report.StatusReports)
            {
                csv.AppendLine($"{item.Status},{item.StatusName},{item.ArticleCount},{item.Percentage}%,{item.LatestArticleDate:yyyy-MM-dd},{item.OldestArticleDate:yyyy-MM-dd}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"news_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private async Task<IActionResult> ExportToJson(ReportViewModel report)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(report, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"news_report_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }
    }
}
