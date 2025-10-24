using System.ComponentModel.DataAnnotations;

namespace ASS1.Models
{
    public class NewsArticleCreateViewModel
    {
        [Required(ErrorMessage = "News title is required")]
        [StringLength(400, ErrorMessage = "News title cannot exceed 400 characters")]
        [Display(Name = "News Title")]
        public string NewsTitle { get; set; } = string.Empty;

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

    public class TagOption
    {
        public int Value { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
