using System.Collections.Generic;
using System.Linq;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    /// <summary>
    ///  Deletion of a category is a cross collection operation.
    /// </summary>
    internal sealed class CategoryRemovalTraverser
    {
        private readonly CategoryLiteDbRepository categoryRepository;
        private readonly IEntityRepository entityRepository;

        internal CategoryRemovalTraverser(CategoryLiteDbRepository categoryRepository, IEntityRepository entityRepository)
        {
            this.categoryRepository = categoryRepository;
            this.entityRepository = entityRepository;
        }

        internal bool DeleteIfEmpty(CategoryModel category)
        {
            if (category.Id == this.categoryRepository.Root().Id)
                return false;

            if (this.SubCategories(category).Any())
                return false;

            if (this.SubEntites(category).Any())
                return false;

            return this.DeleteCategoryInDb(category, recurse: false);
        }

        #region Delete Recursive

        public bool DeleteRecursively(CategoryModel category)
        {
            if (category.Id == this.categoryRepository.Root().Id)
                return false;

            // collect all entites and categories in the given parent category.
            // the delete them
            var entitiesToDelete = new List<EntityModel>();
            var categoriesToDelete = new List<CategoryModel>();

            this.CollectItemsToDelete(category, entitiesToDelete, categoriesToDelete);

            // delete the item from the DB
            foreach (var entityToDelete in entitiesToDelete)
                this.entityRepository.Delete(entityToDelete);
            foreach (var categoryToDelete in categoriesToDelete)
                this.DeleteCategoryInDb(categoryToDelete, recurse: false);
            return this.DeleteCategoryInDb(category, recurse: false);
        }

        private void CollectItemsToDelete(CategoryModel category, List<EntityModel> entitiesToDelete, List<CategoryModel> categoriesToDelete)
        {
            foreach (var subEntity in this.SubEntites(category))
            {
                entitiesToDelete.Add(subEntity);
            }

            foreach (var subCategory in SubCategories(category))
            {
                categoriesToDelete.Add(subCategory);
                this.CollectItemsToDelete(subCategory, entitiesToDelete, categoriesToDelete);
            }
        }

        private bool DeleteCategoryInDb(CategoryModel category, bool recurse) => this.categoryRepository.Delete(category);

        #endregion Delete Recursive

        private IEnumerable<EntityModel> SubEntites(CategoryModel category) => this.entityRepository.FindByCategory(category);

        private IEnumerable<CategoryModel> SubCategories(CategoryModel category) => this.categoryRepository.FindByParent(category);
    }
}