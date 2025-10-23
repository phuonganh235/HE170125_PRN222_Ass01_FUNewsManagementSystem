// NewsService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using Repositories;

namespace Services
{
    public class NewsService : INewsService
    {
        private readonly INewsRepository newsRepo;
        private readonly INewsTagRepository newsTagRepo;

        public NewsService(INewsRepository _newsRepo, INewsTagRepository _newsTagRepo)
        {
            newsRepo = _newsRepo;
            newsTagRepo = _newsTagRepo;
        }

        public IEnumerable<NewsArticle> GetAllNews(string statusFilter = null)
        {
            var all = newsRepo.GetAll();
            if (!string.IsNullOrEmpty(statusFilter))
            {
                all = all.Where(n => n.NewsStatus.Equals(statusFilter, StringComparison.OrdinalIgnoreCase));
            }
            // Có thể sắp xếp bài viết theo ngày tạo hoặc tiêu đề nếu cần:
            return all.OrderByDescending(n => n.CreatedDate);
        }

        public NewsArticle GetNews(int id)
        {
            return newsRepo.GetById(id);
        }

        public bool CreateNews(NewsArticle news, List<int> tagIds, out string error)
        {
            error = string.Empty;
            // Thiết lập thông tin mặc định cho bài viết mới
            news.CreatedDate = DateTime.Now;
            news.ModifiedDate = null;
            news.NewsStatus = string.IsNullOrEmpty(news.NewsStatus) ? "Draft" : news.NewsStatus;
            // Lưu bài viết
            newsRepo.Add(news);
            // Lưu các tag liên quan
            if (tagIds != null)
            {
                foreach (int tagId in tagIds)
                {
                    newsTagRepo.AddTagToNews(news.NewsArticleId, tagId);
                }
            }
            return true;
        }

        public bool UpdateNews(NewsArticle news, List<int> tagIds, out string error)
        {
            error = string.Empty;
            // Cập nhật trường ModifiedDate và UpdatedById (giả sử đã thiết lập UpdatedById bên ngoài trước khi gọi)
            news.ModifiedDate = DateTime.Now;
            // Cập nhật thông tin bài viết
            newsRepo.Update(news);
            // Cập nhật lại tag: Xóa hết tag cũ rồi thêm tag mới
            if (tagIds != null)
            {
                newsTagRepo.RemoveByNews(news.NewsArticleId);
                foreach (int tagId in tagIds)
                {
                    newsTagRepo.AddTagToNews(news.NewsArticleId, tagId);
                }
            }
            return true;
        }

        public bool DeleteNews(int id, out string error)
        {
            error = string.Empty;
            // Xóa liên kết tag trước (để đảm bảo không còn phụ thuộc khóa ngoại)
            newsTagRepo.RemoveByNews(id);
            // Xóa bài viết
            newsRepo.Delete(id);
            return true;
        }

        // Đếm số bài viết theo Category
        public Dictionary<Category, int> CountByCategory()
        {
            var result = new Dictionary<Category, int>();
            // Lấy tất cả bài viết và nhóm theo Category
            var allNews = newsRepo.GetAll();
            var group = allNews.GroupBy(n => n.Category);
            foreach (var g in group)
            {
                result[g.Key] = g.Count();
            }
            return result;
        }

        // Đếm số bài viết theo Status
        public Dictionary<string, int> CountByStatus()
        {
            var result = new Dictionary<string, int>();
            var allNews = newsRepo.GetAll();
            var group = allNews.GroupBy(n => n.NewsStatus);
            foreach (var g in group)
            {
                result[g.Key] = g.Count();
            }
            return result;
        }

        // Đếm số bài viết theo tác giả (CreatedBy)
        public Dictionary<SystemAccount, int> CountByAuthor()
        {
            var result = new Dictionary<SystemAccount, int>();
            var allNews = newsRepo.GetAll();
            var group = allNews.GroupBy(n => n.CreatedBy);
            foreach (var g in group)
            {
                result[g.Key] = g.Count();
            }
            return result;
        }
    }
}
