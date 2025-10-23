// ICategoryService.cs
using System.Collections.Generic;
using BusinessObjects;

namespace Services
{
    public interface ICategoryService
    {
        IEnumerable<Category> GetAllCategories(bool includeInactive = false);
        Category GetCategory(int id);
        bool CreateCategory(Category category, out string error);
        bool UpdateCategory(Category category, out string error);
        bool DeleteCategory(int id, out string error);
    }
}
