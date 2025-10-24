using ASS1.DAL.Models;

namespace ASS1.Models
{
    public class TagSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
