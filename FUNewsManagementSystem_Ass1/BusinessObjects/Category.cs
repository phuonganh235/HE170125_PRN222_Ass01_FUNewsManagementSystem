using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class Category
    {
        public short CategoryId { get; set; } // fix CategoryID
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public short? ParentCategoryId { get; set; }


        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        public ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
    }
}
