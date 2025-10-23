// ITagService.cs
using System.Collections.Generic;
using BusinessObjects;

namespace Services
{
    public interface ITagService
    {
        IEnumerable<Tag> GetAllTags();
        Tag GetTag(int id);
        bool CreateTag(Tag tag, out string error);
        bool UpdateTag(Tag tag, out string error);
        bool DeleteTag(int id, out string error);
    }
}
