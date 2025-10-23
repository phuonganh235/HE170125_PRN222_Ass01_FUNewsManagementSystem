// ICategoryRepository.cs
using System.Collections.Generic;
using BusinessObjects;

namespace Repositories
{
    public interface ICategoryRepository
    {
        IEnumerable<Category> GetAll();
        Category GetById(int categoryId);
        void Add(Category category);
        void Update(Category category);
        void Delete(int categoryId);
    }
}
