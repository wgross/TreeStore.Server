﻿using System.Collections.Generic;
using System.Linq;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    /// <summary>
    ///  Deletion of a category is a cross collection operation.
    /// </summary>
    public sealed class CategoryRemovalTraverser
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IEntityRepository entityRepository;

        public CategoryRemovalTraverser(ICategoryRepository categoryRepository, IEntityRepository entityRepository)
        {
            this.categoryRepository = categoryRepository;
            this.entityRepository = entityRepository;
        }

        public bool DeleteIfEmpty(Category category)
        {
            if (category.Id == this.categoryRepository.Root().Id)
                return false;

            if (this.SubCategories(category).Any())
                return false;

            if (this.SubEntites(category).Any())
                return false;

            return this.DeleteCategoryInDb(category);
        }

        #region Delete Recursive

        public bool DeleteRecursively(Category category)
        {
            if (category.Id == this.categoryRepository.Root().Id)
                return false;

            // collect all entites and categories in the given parent category.
            // the delete them
            var entitiesToDelete = new List<Entity>();
            var categoriesToDelete = new List<Category>();

            this.CollectItemsToDelete(category, entitiesToDelete, categoriesToDelete);

            // delete the item from the DB
            foreach (var entityToDelete in entitiesToDelete)
                this.entityRepository.Delete(entityToDelete);
            foreach (var categoryToDelete in categoriesToDelete)
                this.DeleteCategoryInDb(categoryToDelete);
            return this.DeleteCategoryInDb(category);
        }

        private void CollectItemsToDelete(Category category, List<Entity> entitiesToDelete, List<Category> categoriesToDelete)
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

        private bool DeleteCategoryInDb(Category category) => this.categoryRepository.Delete(category);

        #endregion Delete Recursive

        private IEnumerable<Entity> SubEntites(Category category) => this.entityRepository.FindByCategory(category);

        private IEnumerable<Category> SubCategories(Category category) => this.categoryRepository.FindByParent(category);
    }
}