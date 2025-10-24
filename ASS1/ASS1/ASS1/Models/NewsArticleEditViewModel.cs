using System.ComponentModel.DataAnnotations;

using ASS1.DAL.Models;

namespace ASS1.Models
{
    public class NewsArticleEditViewModel
    {
        [Required]
        public string NewsArticleId { get; set; } = string.Empty;

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
        public bool? NewsStatus { get; set; }

        [Display(Name = "Tags")]
        public List<int> SelectedTagIds { get; set; } = new List<int>();

        public List<CategoryOption> CategoryOptions { get; set; } = new List<CategoryOption>();
        public List<TagOption> TagOptions { get; set; } = new List<TagOption>();
        public List<Tag> CurrentTags { get; set; } = new List<Tag>();
    }
}
