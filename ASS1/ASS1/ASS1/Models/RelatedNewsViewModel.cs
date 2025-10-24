using ASS1.DAL.Models;

namespace ASS1.Models
{
    public class RelatedNewsViewModel
    {
        public NewsArticle CurrentArticle { get; set; } = new NewsArticle();
        public List<NewsArticle> RelatedArticles { get; set; } = new List<NewsArticle>();
        public int RelatedCount { get; set; }
        public bool HasRelatedArticles => RelatedArticles.Any();
    }

    public class NewsArticleWithAuditViewModel
    {
        public NewsArticle Article { get; set; } = new NewsArticle();
        public SystemAccount? LastEditor { get; set; }
        public SystemAccount? Creator { get; set; }
        public bool WasModified => Article.ModifiedDate.HasValue;
        public string LastModifiedBy => LastEditor?.AccountName ?? "Unknown";
        public string CreatedBy => Creator?.AccountName ?? "Unknown";
    }
}
