using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects
{
    public class NewsTag
    {
        [Key, Column(Order = 0)]
        public int NewsArticleId { get; set; }

        [Key, Column(Order = 1)]
        public int TagId { get; set; }

        [ForeignKey("NewsArticleId")]
        public NewsArticle NewsArticle { get; set; }

        [ForeignKey("TagId")]
        public Tag Tag { get; set; }
    }
}
