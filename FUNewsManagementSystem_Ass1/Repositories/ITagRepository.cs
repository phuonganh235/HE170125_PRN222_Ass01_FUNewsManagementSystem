// ITagRepository.cs
using System.Collections.Generic;
using BusinessObjects;

namespace Repositories
{
    public interface ITagRepository
    {
        IEnumerable<Tag> GetAll();
        Tag GetById(int tagId);
        Tag GetByName(string tagName);
        void Add(Tag tag);
        void Update(Tag tag);
        void Delete(int tagId);
    }
}
