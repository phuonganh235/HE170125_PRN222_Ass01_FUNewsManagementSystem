using BusinessObjects;
using Repositories;
using System.Collections.Generic;

namespace Services
{
    public class NewsService : INewsService
    {
        private readonly INewsRepository _newsRepository;

        public NewsService()
        {
            _newsRepository = new NewsRepository();
        }

        public IEnumerable<NewsArticle> GetAll()
        {
            return _newsRepository.GetAll();
        }

        public NewsArticle GetById(string newsId)
        {
            return _newsRepository.GetById(newsId);
        }

        public void Add(NewsArticle article)
        {
            _newsRepository.Add(article);
        }

        public void Update(NewsArticle article)
        {
            _newsRepository.Update(article);
        }

        public void Delete(string newsId)
        {
            _newsRepository.Delete(newsId);
        }

        public bool HasCategory(short categoryId)
        {
            return _newsRepository.HasCategory(categoryId);
        }

        public IEnumerable<NewsArticle> SearchByStaff(string keyword, int staffId)
        {
            return _newsRepository.SearchByStaff(keyword, staffId);
        }

        public NewsArticle Duplicate(string newsId)
        {
            return _newsRepository.Duplicate(newsId);
        }
    }
}