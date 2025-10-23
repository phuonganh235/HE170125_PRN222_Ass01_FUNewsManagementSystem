using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public static class NewsTagDAO
    {
        // Lấy các Tag của một bài viết (trả về danh sách Tag hoặc TagId)
        public static List<Tag> GetTagsByNews(int newsArticleId)
        {
            using (var context = new FUNewsContext())
            {
                return context.NewsTags
                              .Where(nt => nt.NewsArticleId == newsArticleId)
                              .Include(nt => nt.Tag)
                              .Select(nt => nt.Tag)
                              .ToList();
            }
        }

        // Thêm một liên kết Tag vào bài viết
        public static void AddTagToNews(int newsArticleId, int tagId)
        {
            using (var context = new FUNewsContext())
            {
                // Tránh trùng lặp
                var exists = context.NewsTags.Find(newsArticleId, tagId);
                if (exists == null)
                {
                    var newsTag = new NewsTag { NewsArticleId = newsArticleId, TagId = tagId };
                    context.NewsTags.Add(newsTag);
                    context.SaveChanges();
                }
            }
        }

        // Xóa tất cả liên kết Tag của một bài viết (dùng khi xóa bài viết hoặc để cập nhật tag)
        public static void RemoveTagsByNews(int newsArticleId)
        {
            using (var context = new FUNewsContext())
            {
                var links = context.NewsTags.Where(nt => nt.NewsArticleId == newsArticleId).ToList();
                if (links.Any())
                {
                    context.NewsTags.RemoveRange(links);
                    context.SaveChanges();
                }
            }
        }

        // Xóa tất cả liên kết đến một Tag (dùng khi xóa tag)
        public static void RemoveTagsByTag(int tagId)
        {
            using (var context = new FUNewsContext())
            {
                var links = context.NewsTags.Where(nt => nt.TagId == tagId).ToList();
                if (links.Any())
                {
                    context.NewsTags.RemoveRange(links);
                    context.SaveChanges();
                }
            }
        }

        public static bool IsTagUsed(int tagId)
        {
            using var context = new FUNewsContext();
            return context.NewsTags.Any(nt => nt.TagId == tagId);
        }


    }
}
