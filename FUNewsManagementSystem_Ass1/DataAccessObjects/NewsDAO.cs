using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public static class NewsDAO
    {
        // Lấy tất cả bài viết
        public static List<NewsArticle> GetNewsArticles()
        {
            using (var context = new FUNewsContext())
            {
                // Include Category để dùng ngay tên danh mục, Include NewsTags->Tag nếu muốn lấy tag
                return context.NewsArticles
                              .Include(n => n.Category)
                              .Include(n => n.NewsTags).ThenInclude(nt => nt.Tag)
                              .ToList();
            }
        }

        // Lấy bài viết theo ID
        public static NewsArticle FindById(int newsId)
        {
            using (var context = new FUNewsContext())
            {
                return context.NewsArticles
                              .Include(n => n.Category)
                              .Include(n => n.NewsTags).ThenInclude(nt => nt.Tag)
                              .FirstOrDefault(n => n.NewsArticleId == newsId);
            }
        }

        // Lấy các bài viết theo danh mục (cho trang Public hoặc thống kê)
        public static List<NewsArticle> GetByCategory(int categoryId)
        {
            using (var context = new FUNewsContext())
            {
                return context.NewsArticles
                              .Where(n => n.CategoryId == categoryId)
                              .Include(n => n.Category)
                              .ToList();
            }
        }

        // Lấy các bài viết theo người tạo (dùng cho thống kê)
        public static List<NewsArticle> GetByAuthor(int authorId)
        {
            using (var context = new FUNewsContext())
            {
                return context.NewsArticles
                              .Where(n => n.CreatedById == authorId)
                              .Include(n => n.Category)
                              .ToList();
            }
        }

        // Thêm bài viết mới
        public static void Add(NewsArticle news)
        {
            using (var context = new FUNewsContext())
            {
                context.NewsArticles.Add(news);
                context.SaveChanges();
            }
        }

        // Cập nhật bài viết
        public static void Update(NewsArticle news)
        {
            using (var context = new FUNewsContext())
            {
                context.NewsArticles.Update(news);
                context.SaveChanges();
            }
        }

        // Xóa bài viết
        public static void Delete(int newsId)
        {
            using (var context = new FUNewsContext())
            {
                var news = context.NewsArticles.Find(newsId);
                if (news != null)
                {
                    context.NewsArticles.Remove(news);
                    context.SaveChanges();
                }
            }
        }
    }
}
