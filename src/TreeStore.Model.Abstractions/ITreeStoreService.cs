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
        Task<EntityResult> GetEntityByIdAsync(Guid id, CancellationToken cancelled);

        /// <summary>
        /// Creates a new entity from <paramref name="createEntityRequest"/>
        /// </summary>
        Task<EntityResult> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Reads all entities
        /// </summary>
        public Task<IEnumerable<EntityResult>> GetEntitiesAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Creates a new category beneath the parent category <see cref="CreateCategoryRequest.ParentId"/>.
        /// </summary>
        Task<CategoryResult> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the given properties at the given entity
        /// </summary>
        public Task<EntityResult> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Returns the category having the id <paramref name="id"/>.
        /// </summary>
        Task<CategoryResult?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the entity having the <paramref name="id"/>
        /// </summary>
        public Task<bool> DeleteEntityAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the category indentified by <paramref name="id"/> with the changes data defined in <paramref name="request"/>
        /// </summary>
        Task<CategoryResult> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the category identified by <paramref name="id"/>
        /// </summary>
        Task<bool> DeleteCategoryAsync(Guid id, bool recurse, CancellationToken cancellationToken);

        /// <summary>
        /// Copy the <paramref name="sourceCategoryId"/> as s subcategeory to <paramref name="destinationCategoryId"/>.
        /// It <paramref name="recurse"/> is true, all subcatageories and entites are clined as well.
        /// </summary>
        Task<CopyCategoryResponse> CopyCategoryToAsync(Guid sourceCategoryId, Guid destinationCategoryId, bool recurse, CancellationToken cancellationToken);
    }
}