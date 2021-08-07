using System;
using System.Collections.Generic;

namespace TreeStore.Model
{
    public interface ICategoryRepository
    {
        CategoryModel Root();

        CategoryModel FindById(Guid id);

        CategoryModel Upsert(CategoryModel entity);

        CategoryModel? FindByParentAndName(CategoryModel category, string name);

        IEnumerable<CategoryModel> FindByParent(CategoryModel category);

        bool Delete(CategoryModel category, bool recurse);

        /// <summary>
        /// Copy a category <paramref name="sourceCategory"/> to <paramref name="destinationParentCategory"/> as sub catagory.
        /// </summary>
        void CopyTo(CategoryModel sourceCategory, CategoryModel destinationParentCategory, bool recurse);
    }
}