using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    /// <summary>
    /// Implements the TreeStore behavior at the model
    /// </summary>
    public sealed class TreeStoreService : ITreeStoreService
    {
        private readonly ITreeStoreModel model;
        private readonly ILogger<TreeStoreService> logger;

        public TreeStoreService(ITreeStoreModel model, ILogger<TreeStoreService> logger)
        {
            this.model = model;
            this.logger = logger;
        }

        ///<inheritdoc/>
        public Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var parent = this.model.Categories.FindById(request.ParentId);
            if (parent is null)
            {
                this.logger.LogError("Category(name='{categoryName}' wasn't created: Category(id='{parentId}') wasn't found", request.Name, request.ParentId);

                throw new InvalidOperationException($"Category(name='{request.Name}' wasn't created: Category(id='{request.ParentId}') wasn't found");
            }

            var category = new Category
            {
                Name = request.Name,
                Parent = parent
            };

            return Task.FromResult(this.model.Categories.Upsert(category).ToCategoryResponse());
        }

        /// <Inheritdoc/>
        public Task<EntityResponse> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<DeleteCategoryResponse> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
        {
            var category = this.model.Categories.FindById(id);
            if (category is null)
            {
                this.logger.LogInformation("Category(id='{categoryId}') wasn't deleted: Category(id='{categoryId}') doesn't exist", id);

                return Task.FromResult(new DeleteCategoryResponse(Deleted: false));
            }

            return Task.FromResult(new DeleteCategoryResponse(Deleted: this.model.Categories.Delete(category, recurse: false)));
        }

        ///<inheritdoc/>
        public Task<DeleteEntityResponse> DeleteEntityAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<CategoryResponse?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.model.Categories.FindById(id)?.ToCategoryResponse());
        }

        ///<inheritdoc/>
        public Task<IEnumerable<EntityResponse>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<EntityResponse> GetEntityByIdAsync(Guid id, CancellationToken cancelled)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Provides the root <see cref="Category"/> of this model.
        /// </summary>
        public Category GetRootCategory() => this.model.Categories.Root();

        ///<inheritdoc/>
        public Task<CategoryResponse> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = this.model.Categories.FindById(id);
            if (category is null)
            {
                this.logger.LogError("Category(id='{categoryId}') wasn't updated: Category(id='{categoryId}') doesn't exist", id);

                throw new InvalidOperationException($"Category(id='{id}') wasn't updated: Category(id='{id}') doesn't exist");
            }
            category.Name = request.Name;

            return Task.FromResult(this.model.Categories.Upsert(category).ToCategoryResponse());
        }

        ///<inheritdoc/>
        public Task<EntityResponse> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}