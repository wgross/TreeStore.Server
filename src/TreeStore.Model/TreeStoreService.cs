using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    /// <summary>
    /// Implemsn the TreeStore bahavior at the model
    /// </summary>
    public sealed class TreeStoreService : ITreeStoreService
    {
        private readonly ITreeStoreModel model;

        public TreeStoreService(ITreeStoreModel model)
        {
            this.model = model;
        }

        public Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <Inheritdoc/>
        public Task<EntityResponse> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<DeleteCategoryResponse> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<DeleteEntityResponse> DeleteEntityAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CategoryResponse> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

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

        public Task<CategoryResponse> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<EntityResponse> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}