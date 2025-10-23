// NewsRepository.cs
using System.Collections.Generic;
using BusinessObjects;
using DataAccessObjects;

namespace Repositories
{
    public class NewsRepository : INewsRepository
    {
        public IEnumerable<NewsArticle> GetAll()
        {
            return NewsDAO.GetNewsArticles();
        }

        public NewsArticle GetById(int newsId)
        {
            return NewsDAO.FindById(newsId);
        }

        public IEnumerable<NewsArticle> GetByCategory(int categoryId)
        {
            return NewsDAO.GetByCategory(categoryId);
        }

        public IEnumerable<NewsArticle> GetByAuthor(int authorId)
        {
            return NewsDAO.GetByAuthor(authorId);
        }

        public void Add(NewsArticle news)
        {
            NewsDAO.Add(news);
        }

        public void Update(NewsArticle news)
        {
            NewsDAO.Update(news);
        }

        public void Delete(int newsId)
        {
            NewsDAO.Delete(newsId);
        }
    }
}
