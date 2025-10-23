using BusinessObjects;
using System.Collections.Generic;

namespace Services
{
    public interface INewsTagService
    {
        IEnumerable<NewsTag> GetAll();
        void Add(NewsTag newsTag);
        void Delete(string newsId, int tagId);
        IEnumerable<Tag> GetTagsByNewsId(string newsId);
    }
}
