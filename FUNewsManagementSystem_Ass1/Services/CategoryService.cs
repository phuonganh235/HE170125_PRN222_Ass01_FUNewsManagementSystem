// CategoryService.cs
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using Repositories;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository categoryRepo;
        private readonly INewsRepository newsRepo;

        public CategoryService(ICategoryRepository _categoryRepo, INewsRepository _newsRepo)
        {
            categoryRepo = _categoryRepo;
            newsRepo = _newsRepo;
        }

        public IEnumerable<Category> GetAllCategories(bool includeInactive = false)
        {
            var all = categoryRepo.GetAll();
            if (!includeInactive)
            {
                // Chỉ lấy các category đang hoạt động nếu cần
                all = all.Where(c => c.IsActive);
            }
            // Có thể sort theo tên nếu muốn
            return all;
        }

        public Category GetCategory(int id)
        {
            return categoryRepo.GetById(id);
        }

        public bool CreateCategory(Category category, out string error)
        {
            error = string.Empty;
            // Kiểm tra trùng tên danh mục (không phân biệt hoa thường)
            var all = categoryRepo.GetAll();
            if (all.Any(c => c.CategoryName.ToLower() == category.CategoryName.ToLower()))
            {
                error = "Tên danh mục đã tồn tại!";
                return false;
            }
            // Nếu có parentId, kiểm tra parent có tồn tại và đang active không (có thể bổ sung nếu cần)
            categoryRepo.Add(category);
            return true;
        }

        public bool UpdateCategory(Category category, out string error)
        {
            error = string.Empty;
            // Kiểm tra trùng tên (cho trường hợp đổi tên)
            var all = categoryRepo.GetAll();
            if (all.Any(c => c.CategoryName.ToLower() == category.CategoryName.ToLower() && c.CategoryId != category.CategoryId))
            {
                error = "Tên danh mục đã tồn tại ở danh mục khác!";
                return false;
            }
            categoryRepo.Update(category);
            return true;
        }

        public bool DeleteCategory(int id, out string error)
        {
            error = string.Empty;
            // Không cho xóa nếu có danh mục con
            var all = categoryRepo.GetAll();
            if (all.Any(c => c.ParentCategoryId == id))
            {
                error = "Không thể xóa vì còn danh mục con phụ thuộc!";
                return false;
            }
            // Không cho xóa nếu có bài viết thuộc danh mục
            var newsList = newsRepo.GetByCategory(id);
            if (newsList != null && newsList.Any())
            {
                error = "Không thể xóa vì danh mục đang được sử dụng cho các bài viết!";
                return false;
            }
            categoryRepo.Delete(id);
            return true;
        }
    }
}
