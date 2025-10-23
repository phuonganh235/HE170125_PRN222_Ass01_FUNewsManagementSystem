using BusinessObjects;
using Repositories;
using System.Collections.Generic;

namespace Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService()
        {
            _tagRepository = new TagRepository();
        }

        public IEnumerable<Tag> GetAll()
        {
            return _tagRepository.GetAll();
        }

        public Tag GetById(int id)
        {
            return _tagRepository.GetById(id);
        }

        public void Add(Tag tag)
        {
            _tagRepository.Add(tag);
        }

        public void Update(Tag tag)
        {
            _tagRepository.Update(tag);
        }

        public void Delete(int id)
        {
            _tagRepository.Delete(id);
        }

        public IEnumerable<Tag> Search(string keyword)
        {
            return _tagRepository.Search(keyword);
        }

        public bool IsDuplicate(string tagName)
        {
            return _tagRepository.IsDuplicate(tagName);
        }
    }
}
