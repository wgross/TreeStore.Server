using System;
using System.Collections.Generic;

namespace TreeStore.Model
{
    public interface ICategoryRepository
    {
        Category Root();

        Category FindById(Guid id);

        Category Upsert(Category entity);

        Category? FindByParentAndName(Category category, string name);

        IEnumerable<Category> FindByParent(Category category);

        bool Delete(Category category, bool recurse);

        /// <summary>
        /// Copy a category <paramref name="sourceCategory"/> to <paramref name="destinationParentCategory"/> as sub catagory.
        /// </summary>
        void CopyTo(Category sourceCategory, Category destinationParentCategory, bool recurse);
    }
}