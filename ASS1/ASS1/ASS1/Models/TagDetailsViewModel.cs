using ASS1.DAL.Models;

namespace ASS1.Models
{
    public class TagDetailsViewModel
    {
        public Tag Tag { get; set; } = new Tag();
        public List<NewsArticle> Articles { get; set; } = new List<NewsArticle>();
        public int ArticleCount { get; set; }
        public bool CanDelete { get; set; }
    }
}
