using ASS1.DAL.Models;

namespace ASS1.Models
{
    public class CategorySearchViewModel
    {
        public string? SearchTerm { get; set; }
        public bool? IsActiveFilter { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
