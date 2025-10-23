using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string CategoryName { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;  // mặc định là kích hoạt

        public int? ParentCategoryId { get; set; }

        [ForeignKey("ParentCategoryId")]
        public Category ParentCategory { get; set; }  // Danh mục cha (nếu có)

        public ICollection<Category> SubCategories { get; set; }  // Các danh mục con

        public ICollection<NewsArticle> NewsArticles { get; set; }  // Các bài viết thuộc danh mục này
    }
}
