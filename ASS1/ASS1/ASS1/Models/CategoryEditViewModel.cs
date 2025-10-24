using System.ComponentModel.DataAnnotations;

namespace ASS1.Models
{
    public class CategoryEditViewModel
    {
        [Required]
        public short CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category description is required")]
        [StringLength(250, ErrorMessage = "Category description cannot exceed 250 characters")]
        [Display(Name = "Category Description")]
        public string CategoryDesciption { get; set; } = string.Empty;

        [Display(Name = "Parent Category")]
        public short? ParentCategoryId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public bool CanChangeParentCategory { get; set; } = true;
        public List<CategoryOption> ParentCategoryOptions { get; set; } = new List<CategoryOption>();
    }
}
