// INewsService.cs
using System.Collections.Generic;
using BusinessObjects;

namespace Services
{
    public interface INewsService
    {
        IEnumerable<NewsArticle> GetAllNews(string statusFilter = null);
        NewsArticle GetNews(int id);
        bool CreateNews(NewsArticle news, List<int> tagIds, out string error);
        bool UpdateNews(NewsArticle news, List<int> tagIds, out string error);
        bool DeleteNews(int id, out string error);
        // Các hàm hỗ trợ báo cáo:
        Dictionary<Category, int> CountByCategory();
        Dictionary<string, int> CountByStatus();
        Dictionary<SystemAccount, int> CountByAuthor();
    }
}
