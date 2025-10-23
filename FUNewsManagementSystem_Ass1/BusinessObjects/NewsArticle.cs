using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class NewsArticle
    {
        [Key]
        public int NewsArticleId { get; set; }

        [Required(ErrorMessage = "Tiêu đề không được để trống")]
        [StringLength(200)]
        public string NewsTitle { get; set; }

        [Required(ErrorMessage = "Nội dung không được để trống")]
        public string Content { get; set; }

        [Required]
        [StringLength(20)]
        public string NewsStatus { get; set; }   // VD: "Draft" hoặc "Published"

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public int CreatedById { get; set; }
        public int? UpdatedById { get; set; }

        [ForeignKey("CreatedById")]
        public SystemAccount CreatedBy { get; set; }

        [ForeignKey("UpdatedById")]
        public SystemAccount UpdatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation to tags (many-to-many via NewsTag)
        public ICollection<NewsTag> NewsTags { get; set; }
    }
}
