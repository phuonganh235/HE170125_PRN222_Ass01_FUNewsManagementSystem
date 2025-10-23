using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class SystemAccount
    {
        [Key]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Tên tài khoản không được trống")]
        [StringLength(100)]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "Email không được trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100)]
        public string AccountEmail { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được trống")]
        [StringLength(100)]
        public string AccountPassword { get; set; }

        [Required]
        [StringLength(20)]
        public string AccountRole { get; set; }

        // Navigation properties:
        public ICollection<NewsArticle> CreatedNews { get; set; }  // Các bài viết do tài khoản tạo
        public ICollection<NewsArticle> UpdatedNews { get; set; }  // Các bài viết tài khoản cập nhật
    }
}
