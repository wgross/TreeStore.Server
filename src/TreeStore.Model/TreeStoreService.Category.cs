using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Common;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    /// <summary>
    /// Implements the TreeStore behavior at the model
    /// </summary>
    public sealed partial class TreeStoreService : ITreeStoreService
    {
        private readonly ITreeStoreModel model;
        private readonly ILogger<TreeStoreService> logger;

        public TreeStoreService(ITreeStoreModel model, ILogger<TreeStoreService> logger)
        {
            this.model = model;
            this.logger = logger;
        }

        public Task<CategoryResult> CopyCategoryToAsync(Guid sourceCategoryId, Guid destinationCategoryId, bool recurse, CancellationToken cancellationToken)
        {
            var sourceCategory = this.model.Categories.FindById(sourceCategoryId);
            if (sourceCategory is null)
                throw new InvalidOperationException($"Category(id='{sourceCategoryId}') wasn't copied: Category(id='{sourceCategoryId}') doesn't exist");

            var destinationCategory = this.model.Categories.FindById(destinationCategoryId);
            if (destinationCategory is null)
                throw new InvalidOperationException($"Category(id='{sourceCategoryId}') wasn't copied: Category(id='{destinationCategoryId}') doesn't exist");

            var category = this.model.Categories.CopyTo(sourceCategory, destinationCategory, recurse);

            return Task.FromResult(category.ToCategoryResult());
        }

        /// <inheritdoc/>
        public Task<CategoryResult> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var parent = this.model.Categories.FindById(request.ParentId);
            if (parent is null)
            {
                this.logger.LogError("Category(name='{categoryName}' wasn't created: Category(id='{parentId}') wasn't found", request.Name, request.ParentId);

                throw new InvalidOperationException($"Category(name='{request.Name}' wasn't created: Category(id='{request.ParentId}') wasn't found");
            }

            var category = this.Apply(request, new CategoryModel());

            parent.AddSubCategory(category);

            return Task.FromResult(this.model.Categories.Upsert(category).ToCategoryResult());
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCategoryAsync(Guid id, bool recurse, CancellationToken cancellationToken)
        {
            var category = this.model.Categories.FindById(id);
            if (category is null)
            {
                this.logger.LogInformation("Category(id='{categoryId}') wasn't deleted: Category(id='{categoryId}') doesn't exist", id);

                return Task.FromResult(false);
            }

            return Task.FromResult(this.model.Categories.Delete(category, recurse: recurse));
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCategoryAsync(Guid parentId, string childName, bool recurse, CancellationToken cancellationToken)
        {
            Guard.AgainstNull(childName, nameof(childName));

            var parentCategory = this.model.Categories.FindById(parentId);
            if (parentCategory is null)
            {
                this.logger.LogInformation("Category(parentId='{parentId}',name='{childName}') wasn't deleted: Parent doesn't exist", parentId, childName);

                return Task.FromResult(false);
            }

            var childCategory = this.model.Categories.FindByParentAndName(parentCategory!, childName);
            if (childCategory is null)
            {
                this.logger.LogInformation("Category(parentId='{parentId}',name='{childName}') wasn't deleted: It doesn't exist", parentId, childName);

                return Task.FromResult(false);
            }

            return Task.FromResult(this.model.Categories.Delete(childCategory!, recurse));
        }

        /// <inheritdoc/>
        public Task<CategoryResult?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken) => this.GetCategoryByIdImplAsync(id);

        private Task<CategoryResult?> GetCategoryByIdImplAsync(Guid id)
        {
            var category = this.model.Categories.FindById(id);

            if (category is null)
            {
                this.logger.LogInformation("Category(id='{id}') wasn't found.", id);

                return Task.FromResult((CategoryResult?)null);
            }

            var subcategories = this.model.Categories.FindByParent(category);
            var entities = this.model.Entities.FindByCategory(category);

            return Task.FromResult(category?.ToCategoryResult(subcategories, entities));
        }

        /// <inheritdoc/>

        public Task<IEnumerable<CategoryResult>?> GetCategoriesByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var parentCategory = this.model.Categories.FindById(id);
            if (parentCategory is null)
            {
                this.logger.LogInformation("Children of Category(id='{id}') weren't read: parent doesn't exist", id);

                return Task.FromResult((IEnumerable<CategoryResult>?)null);
            }
            return Task.FromResult(this.model.Categories.FindByParent(parentCategory).Select(c => c.ToCategoryResult()));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Task<CategoryResult> GetRootCategoryAsync(CancellationToken cancellationToken) => this.GetCategoryByIdImplAsync(this.model.Categories.Root().Id)!;

        /// <inheritdoc/>
        public Task<CategoryResult> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = this.model.Categories.FindById(id);
            if (category is null)
            {
                this.logger.LogError("Category(id='{categoryId}') wasn't updated: Category(id='{categoryId}') doesn't exist", id);

                throw new InvalidOperationException($"Category(id='{id}') wasn't updated: Category(id='{id}') doesn't exist");
            }

            this.Apply(request, category);

            return Task.FromResult(this.model.Categories.Upsert(category).ToCategoryResult());
        }

        #region Apply

        public void Apply(UpdateCategoryRequest updateCategoryRequest, CategoryModel category)
        {
            category.Name = updateCategoryRequest.Name ?? category.Name;

            if (updateCategoryRequest.Facet is not null)
                this.Apply(updateCategoryRequest.Facet, category.Facet);
        }

        private CategoryModel Apply(CreateCategoryRequest request, CategoryModel category)
        {
            category.Name = request.Name ?? category.Name;

            if (request.Facet is not null)
                this.Apply(request.Facet, category.Facet);

            return category;
        }

        #endregion Apply
    }
}