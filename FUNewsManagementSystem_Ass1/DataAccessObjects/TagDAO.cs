using System.Collections.Generic;
using System.Linq;
using BusinessObjects;

namespace DataAccessObjects
{
    public static class TagDAO
    {
        public static List<Tag> GetTags()
        {
            using (var context = new FUNewsContext())
            {
                return context.Tags.ToList();
            }
        }

        public static Tag FindById(int tagId)
        {
            using (var context = new FUNewsContext())
            {
                return context.Tags.Find(tagId);
            }
        }

        public static Tag FindByName(string tagName)
        {
            using (var context = new FUNewsContext())
            {
                return context.Tags.FirstOrDefault(t => t.TagName.ToLower() == tagName.ToLower());
            }
        }

        public static void Add(Tag tag)
        {
            using (var context = new FUNewsContext())
            {
                context.Tags.Add(tag);
                context.SaveChanges();
            }
        }

        public static void Update(Tag tag)
        {
            using (var context = new FUNewsContext())
            {
                context.Tags.Update(tag);
                context.SaveChanges();
            }
        }

        public static void Delete(int tagId)
        {
            using (var context = new FUNewsContext())
            {
                var tag = context.Tags.Find(tagId);
                if (tag != null)
                {
                    context.Tags.Remove(tag);
                    context.SaveChanges();
                }
            }
        }

        public static bool IsTagUsed(int tagId)
        {
            using var ctx = new FUNewsContext();
            return ctx.NewsTags.Any(nt => nt.TagId == tagId);
        }

    }
}
