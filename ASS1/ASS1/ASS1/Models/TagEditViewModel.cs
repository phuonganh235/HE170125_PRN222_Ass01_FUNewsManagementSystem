using System.ComponentModel.DataAnnotations;

namespace ASS1.Models
{
    public class TagEditViewModel
    {
        [Required]
        public int TagId { get; set; }

        [Required(ErrorMessage = "Tag name is required")]
        [StringLength(50, ErrorMessage = "Tag name cannot exceed 50 characters")]
        [Display(Name = "Tag Name")]
        public string TagName { get; set; } = string.Empty;

        [StringLength(400, ErrorMessage = "Note cannot exceed 400 characters")]
        [Display(Name = "Note")]
        public string? Note { get; set; }
    }
}
