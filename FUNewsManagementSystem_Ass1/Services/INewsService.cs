using BusinessObjects;
using System;
using System.Collections.Generic;

namespace Services
{
    public interface INewsService
    {
        IEnumerable<NewsArticle> GetAll();
        NewsArticle GetById(string newsId);
        void Add(NewsArticle article);
        void Update(NewsArticle article);
        void Delete(string newsId);
        bool HasCategory(short categoryId);
        IEnumerable<NewsArticle> SearchByStaff(string keyword, int staffId);
        NewsArticle Duplicate(string newsId);
    }
}
