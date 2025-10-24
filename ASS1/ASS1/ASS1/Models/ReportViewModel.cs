using System.ComponentModel.DataAnnotations;

namespace ASS1.Models
{
    public class ReportViewModel
    {
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime? FromDate { get; set; }

        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime? ToDate { get; set; }

        public List<CategoryReportItem> CategoryReports { get; set; } = new List<CategoryReportItem>();
        public List<AuthorReportItem> AuthorReports { get; set; } = new List<AuthorReportItem>();
        public List<StatusReportItem> StatusReports { get; set; } = new List<StatusReportItem>();
        public ReportSummary Summary { get; set; } = new ReportSummary();
    }

    public class CategoryReportItem
    {
        public short CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public DateTime? LatestArticleDate { get; set; }
        public DateTime? OldestArticleDate { get; set; }
    }

    public class AuthorReportItem
    {
        public short AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorEmail { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public DateTime? LatestArticleDate { get; set; }
        public DateTime? OldestArticleDate { get; set; }
    }

    public class StatusReportItem
    {
        public bool Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int ArticleCount { get; set; }
        public double Percentage { get; set; }
        public DateTime? LatestArticleDate { get; set; }
        public DateTime? OldestArticleDate { get; set; }
    }

    public class ReportSummary
    {
        public int TotalArticles { get; set; }
        public int TotalActiveArticles { get; set; }
        public int TotalInactiveArticles { get; set; }
        public double ActivePercentage { get; set; }
        public double InactivePercentage { get; set; }
        public int TotalCategories { get; set; }
        public int TotalAuthors { get; set; }
        public DateTime? ReportGeneratedDate { get; set; }
        public DateTime? DateRangeStart { get; set; }
        public DateTime? DateRangeEnd { get; set; }
    }
}
