using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class NewsArticle
    {
        public int NewsArticleId { get; set; }
        public string NewsTitle { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string NewsSource { get; set; } = string.Empty;
        public string NewsStatus { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }


        public short CategoryId { get; set; }
        public Category Category { get; set; } = null!;


        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }


        public SystemAccount CreatedNews { get; set; } = null!;
        public SystemAccount? UpdatedNews { get; set; }


        public ICollection<NewsTag> NewsTags { get; set; } = new List<NewsTag>();
    }
}
