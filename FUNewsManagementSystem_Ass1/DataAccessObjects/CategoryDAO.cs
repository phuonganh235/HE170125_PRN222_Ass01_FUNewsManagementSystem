using System.Collections.Generic;
using System.Linq;
using BusinessObjects;

namespace DataAccessObjects
{
    public static class CategoryDAO
    {
        // Lấy tất cả danh mục (có thể bao gồm cả Inactive)
        public static List<Category> GetCategories()
        {
            using (var context = new FUNewsContext())
            {
                // Include SubCategories if needed: .Include(c => c.SubCategories)
                return context.Categories.ToList();
            }
        }

        // Lấy danh mục theo ID
        public static Category FindById(int categoryId)
        {
            using (var context = new FUNewsContext())
            {
                return context.Categories.Find(categoryId);
            }
        }

        // Thêm danh mục mới
        public static void Add(Category category)
        {
            using (var context = new FUNewsContext())
            {
                context.Categories.Add(category);
                context.SaveChanges();
            }
        }

        // Cập nhật danh mục
        public static void Update(Category category)
        {
            using (var context = new FUNewsContext())
            {
                context.Categories.Update(category);
                context.SaveChanges();
            }
        }

        // Xóa danh mục
        public static void Delete(int categoryId)
        {
            using (var context = new FUNewsContext())
            {
                var category = context.Categories.Find(categoryId);
                if (category != null)
                {
                    context.Categories.Remove(category);
                    context.SaveChanges();
                }
            }
        }
    }
}
