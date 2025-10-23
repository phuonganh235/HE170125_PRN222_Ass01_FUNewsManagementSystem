// NewsTagRepository.cs
using System.Collections.Generic;
using BusinessObjects;
using DataAccessObjects;

namespace Repositories
{
    public class NewsTagRepository : INewsTagRepository
    {
        public IEnumerable<Tag> GetTagsByNews(int newsId)
        {
            return NewsTagDAO.GetTagsByNews(newsId);
        }

        public void AddTagToNews(int newsId, int tagId)
        {
            NewsTagDAO.AddTagToNews(newsId, tagId);
        }

        public void RemoveByNews(int newsId)
        {
            NewsTagDAO.RemoveTagsByNews(newsId);
        }

        public void RemoveByTag(int tagId)
        {
            NewsTagDAO.RemoveTagsByTag(tagId);
        }
        public bool IsTagUsed(int tagId)
        {
            return NewsTagDAO.IsTagUsed(tagId);
        }

    }
}
