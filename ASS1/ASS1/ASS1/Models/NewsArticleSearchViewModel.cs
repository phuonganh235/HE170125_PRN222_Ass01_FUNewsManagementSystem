using ASS1.DAL.Models;

namespace ASS1.Models
{
    public class NewsArticleSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public string? AuthorName { get; set; }
        public string? CategoryName { get; set; }
        public bool? StatusFilter { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public List<NewsArticle> Articles { get; set; } = new List<NewsArticle>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;
    }
}
