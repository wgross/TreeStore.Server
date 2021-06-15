using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TreeStore.Model.Abstractions
{
    public interface ITreeStoreService
    {
        /// <summary>
        /// Retrieve the state of the entity having the id <paramref name="id"/>.
        /// </summary>
        Task<EntityResponse> GetEntityByIdAsync(Guid id, CancellationToken cancelled);

        /// <summary>
        /// Creates a new entity from <paramref name="createEntityRequest"/>
        /// </summary>
        Task<EntityResponse> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Reads all entities
        /// </summary>
        public Task<IEnumerable<EntityResponse>> GetEntitiesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new category beneath the parent category <see cref="CreateCategoryRequest.ParentId"/>.
        /// </summary>
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the given properties at the given entity
        /// </summary>
        public Task<EntityResponse> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Returns the category having the id <paramref name="id"/>.
        /// </summary>
        Task<CategoryResponse?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the entity having the <paramref name="id"/>
        /// </summary>
        public Task<DeleteEntityResponse> DeleteEntityAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the category indentified by <paramref name="id"/> with the changes data defined in <paramref name="request"/>
        /// </summary>
        Task<CategoryResponse> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the category identified by <paramref name="id"/>
        /// </summary>
        Task<DeleteCategoryResponse> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
    }
}