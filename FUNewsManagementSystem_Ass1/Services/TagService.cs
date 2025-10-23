// TagService.cs
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using Repositories;

namespace Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository tagRepo;
        private readonly INewsTagRepository newsTagRepo;

        public TagService(ITagRepository _tagRepo, INewsTagRepository _newsTagRepo)
        {
            tagRepo = _tagRepo;
            newsTagRepo = _newsTagRepo;
        }

        public IEnumerable<Tag> GetAllTags()
        {
            return tagRepo.GetAll();
        }

        public Tag GetTag(int id)
        {
            return tagRepo.GetById(id);
        }

        public bool CreateTag(Tag tag, out string error)
        {
            error = string.Empty;
            // Kiểm tra trùng tên tag
            var exist = tagRepo.GetByName(tag.TagName);
            if (exist != null)
            {
                error = "Tên thẻ đã tồn tại!";
                return false;
            }
            tagRepo.Add(tag);
            return true;
        }

        public bool UpdateTag(Tag tag, out string error)
        {
            error = string.Empty;
            var exist = tagRepo.GetByName(tag.TagName);
            if (exist != null && exist.TagId != tag.TagId)
            {
                error = "Tên thẻ đã tồn tại ở thẻ khác!";
                return false;
            }
            tagRepo.Update(tag);
            return true;
        }

        public bool DeleteTag(int id, out string error)
        {
            error = string.Empty;
            // Không cho xóa nếu thẻ đang gắn với bài viết nào
            if (newsTagRepo.IsTagUsed(id))
            {
                error = "Không thể xóa thẻ vì đang được sử dụng trong bài viết!";
                return false;
            }
            tagRepo.Delete(id);
            return true;
        }
    }
}
