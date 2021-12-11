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
                this.LogCreatingCategoryFailedMissingParent(request.Name, request.ParentId);

                throw new InvalidOperationException($"Category(name='{request.Name}' wasn't created: Category(id='{request.ParentId}') wasn't found");
            }

            var category = this.Apply(request, new CategoryModel());

            parent.AddSubCategory(category);

            this.model.Categories.Upsert(category);

            this.LogCategoryCreated(category.Id, category.Parent!.Id, category.Name);

            return Task.FromResult(category.ToCategoryResult());
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCategoryAsync(Guid id, bool recurse, CancellationToken cancellationToken)
        {
            var category = this.model.Categories.FindById(id);
            if (category is null)
            {
                this.LogDeletingCategoryFailed(id);

                return Task.FromResult(false);
            }

            return Task.FromResult(this.model.Categories.Delete(category, recurse: recurse));
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCategoryAsync(Guid parentId, string childName, bool recurse, CancellationToken cancellationToken)
        {
            var parentCategory = this.model.Categories.FindById(parentId);
            if (parentCategory is null)
            {
                this.LogDeletingCategoryByNameFailedMissingParent(parentId, childName);

                return Task.FromResult(false);
            }

            var childCategory = this.model.Categories.FindByParentAndName(parentCategory!, Guard.Against.Null(childName, nameof(childName)));
            if (childCategory is null)
            {
                this.LogDeletingCategoryByNameFailedMissingChild(parentId, childName);

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
                this.LogReadingCategoryFailedMissing(id);

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
                this.LogReadingCategoryChildrenFailedMissingParent(id);

                return Task.FromResult((IEnumerable<CategoryResult>?)null);
            }
            return Task.FromResult<IEnumerable<CategoryResult>?>(this.model.Categories.FindByParent(parentCategory).Select(c => c.ToCategoryResult()));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public Task<CategoryResult?> GetRootCategoryAsync(CancellationToken cancellationToken) => this.GetCategoryByIdImplAsync(this.model.Categories.Root().Id)!;

        /// <inheritdoc/>
        public Task<CategoryResult> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = this.model.Categories.FindById(id);
            if (category is null)
            {
                this.LogUpdatingCategoryFailedMissing(id);

                throw new InvalidOperationException($"Category(id='{id}') wasn't updated: Category(id='{id}') doesn't exist");
            }

            if (request.Name is not null)
            {
                // name is about to be updated.
                // check if there is an entity having the same name
                var entity = this.model.Entities.FindByCategoryAndName(category.Parent!, request.Name);
                if (entity is not null)
                {
                    this.LogUpdatingCategoryFailedDuplicateName(id, request.Name, entity.Id);

                    throw new InvalidOperationException($"Category(id='{category.Id}') wasn't updated: duplicate name with Entity(id='{entity.Id}')");
                }
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