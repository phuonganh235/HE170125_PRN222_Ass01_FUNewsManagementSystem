// TagRepository.cs
using System.Collections.Generic;
using BusinessObjects;
using DataAccessObjects;

namespace Repositories
{
    public class TagRepository : ITagRepository
    {
        public IEnumerable<Tag> GetAll()
        {
            return TagDAO.GetTags();
        }

        public Tag GetById(int tagId)
        {
            return TagDAO.FindById(tagId);
        }

        public Tag GetByName(string tagName)
        {
            return TagDAO.FindByName(tagName);
        }

        public void Add(Tag tag)
        {
            TagDAO.Add(tag);
        }

        public void Update(Tag tag)
        {
            TagDAO.Update(tag);
        }

        public void Delete(int tagId)
        {
            TagDAO.Delete(tagId);
        }
    }
}
