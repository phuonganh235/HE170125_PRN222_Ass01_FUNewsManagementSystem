using System.ComponentModel.DataAnnotations;

namespace ASS1.Models
{
    public class NewsTagLinkViewModel
    {
        [Required]
        public string NewsArticleId { get; set; } = string.Empty;

        [Required]
        public int TagId { get; set; }

        public string TagName { get; set; } = string.Empty;
        public string ArticleTitle { get; set; } = string.Empty;
    }

    public class ArticleTagManagementViewModel
    {
        [Required]
        public string NewsArticleId { get; set; } = string.Empty;

        public string ArticleTitle { get; set; } = string.Empty;
        public List<NewsTagLinkViewModel> CurrentTags { get; set; } = new List<NewsTagLinkViewModel>();
        public List<TagOption> AvailableTags { get; set; } = new List<TagOption>();
        public List<int> SelectedTagIds { get; set; } = new List<int>();
    }

    public class TagAssignmentViewModel
    {
        [Required]
        public string NewsArticleId { get; set; } = string.Empty;

        [Required]
        public int TagId { get; set; }

        public string TagName { get; set; } = string.Empty;
        public string ArticleTitle { get; set; } = string.Empty;
    }
}
