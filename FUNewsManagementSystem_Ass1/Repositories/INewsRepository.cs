// INewsRepository.cs
using System.Collections.Generic;
using BusinessObjects;

namespace Repositories
{
    public interface INewsRepository
    {
        IEnumerable<NewsArticle> GetAll();                 // Lấy tất cả bài viết
        NewsArticle GetById(int newsId);
        IEnumerable<NewsArticle> GetByCategory(int categoryId);
        IEnumerable<NewsArticle> GetByAuthor(int authorId);
        void Add(NewsArticle news);
        void Update(NewsArticle news);
        void Delete(int newsId);
    }
}
