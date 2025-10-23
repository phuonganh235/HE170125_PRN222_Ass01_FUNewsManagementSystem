using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [Required(ErrorMessage = "Tên thẻ không được để trống")]
        [StringLength(100)]
        public string TagName { get; set; }

        [StringLength(255)]
        public string Note { get; set; }

        public ICollection<NewsTag> NewsTags { get; set; }  // Danh sách bài viết gắn thẻ này
    }
}
