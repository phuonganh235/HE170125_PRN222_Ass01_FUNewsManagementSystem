// CategoryRepository.cs
using System.Collections.Generic;
using BusinessObjects;
using DataAccessObjects;

namespace Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        public IEnumerable<Category> GetAll()
        {
            return CategoryDAO.GetCategories();
        }

        public Category GetById(int categoryId)
        {
            return CategoryDAO.FindById(categoryId);
        }

        public void Add(Category category)
        {
            CategoryDAO.Add(category);
        }

        public void Update(Category category)
        {
            CategoryDAO.Update(category);
        }

        public void Delete(int categoryId)
        {
            CategoryDAO.Delete(categoryId);
        }
    }
}
