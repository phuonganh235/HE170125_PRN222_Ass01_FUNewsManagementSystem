using BusinessObjects;
using Repositories;
using System.Collections.Generic;

namespace Services
{
    public class NewsTagService : INewsTagService
    {
        private readonly INewsTagRepository _newsTagRepository;

        public NewsTagService()
        {
            _newsTagRepository = new NewsTagRepository();
        }

        public IEnumerable<NewsTag> GetAll()
        {
            return _newsTagRepository.GetAll();
        }

        public void Add(NewsTag newsTag)
        {
            _newsTagRepository.Add(newsTag);
        }

        public void Delete(string newsId, int tagId)
        {
            _newsTagRepository.Delete(newsId, tagId);
        }

        public IEnumerable<Tag> GetTagsByNewsId(string newsId)
        {
            return _newsTagRepository.GetTagsByNewsId(newsId);
        }
    }
}
