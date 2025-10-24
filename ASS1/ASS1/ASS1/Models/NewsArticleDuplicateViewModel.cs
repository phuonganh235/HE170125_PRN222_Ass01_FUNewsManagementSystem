using System.ComponentModel.DataAnnotations;

namespace ASS1.Models
{
    public class NewsArticleDuplicateViewModel
    {
        [Required]
        public string OriginalNewsArticleId { get; set; } = string.Empty;

        [Required(ErrorMessage = "New news article ID is required")]
        [StringLength(20, ErrorMessage = "News article ID cannot exceed 20 characters")]
        [Display(Name = "New News Article ID")]
        public string NewNewsArticleId { get; set; } = string.Empty;

        [StringLength(400, ErrorMessage = "News title cannot exceed 400 characters")]
        [Display(Name = "News Title")]
        public string? NewsTitle { get; set; }

        [Required(ErrorMessage = "Headline is required")]
        [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters")]
        [Display(Name = "Headline")]
        public string Headline { get; set; } = string.Empty;

        [StringLength(4000, ErrorMessage = "News content cannot exceed 4000 characters")]
        [Display(Name = "News Content")]
        public string? NewsContent { get; set; }

        [StringLength(400, ErrorMessage = "News source cannot exceed 400 characters")]
        [Display(Name = "News Source")]
        public string? NewsSource { get; set; }

        [Display(Name = "Category")]
        public short? CategoryId { get; set; }

        [Display(Name = "Status")]
        public bool NewsStatus { get; set; } = true;

        [Display(Name = "Tags")]
        public List<int> SelectedTagIds { get; set; } = new List<int>();

        public List<CategoryOption> CategoryOptions { get; set; } = new List<CategoryOption>();
        public List<TagOption> TagOptions { get; set; } = new List<TagOption>();
    }
}
