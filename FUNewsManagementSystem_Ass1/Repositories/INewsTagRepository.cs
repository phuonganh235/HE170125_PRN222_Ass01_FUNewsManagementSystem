// INewsTagRepository.cs
using System.Collections.Generic;
using BusinessObjects;

namespace Repositories
{
    public interface INewsTagRepository
    {
        IEnumerable<Tag> GetTagsByNews(int newsId);
        void AddTagToNews(int newsId, int tagId);
        void RemoveByNews(int newsId);
        void RemoveByTag(int tagId);
        bool IsTagUsed(int tagId);
    }
}
