using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Tag
    {
        public int TagId { get; set; }
        public string TagName { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;


        public ICollection<NewsTag> NewsTags { get; set; } = new List<NewsTag>();
    }
}
