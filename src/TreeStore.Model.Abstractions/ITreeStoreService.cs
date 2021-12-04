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
        Task<EntityResult?> GetEntityByIdAsync(Guid id, CancellationToken cancelled);

        /// <summary>
        /// Creates a new entity from <paramref name="createEntityRequest"/>
        /// </summary>
        Task<EntityResult?> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken);

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
        Task<EntityResult> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the given properties at the given tag.
        /// </summary>
        Task<TagResult> CreateTagAsync(CreateTagRequest createTagRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the root <see cref="CategoryResult"/>. The model creates the root by itself.
        /// It does always exits and cant't be deleted.
        /// </summary>
        Task<CategoryResult?> GetRootCategoryAsync(CancellationToken none);

        /// <summary>
        /// Returns the category having the id <paramref name="id"/>.
        /// </summary>
        Task<CategoryResult?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the child categories of the specified category <paramref name="id"/>.
        /// It returns null if the parent category doesn't exist.
        /// </summary>
        Task<IEnumerable<CategoryResult>?> GetCategoriesByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the entity having the <paramref name="id"/>
        /// </summary>
        Task<bool> DeleteEntityAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the specifed Tag
        /// </summary>
        Task<TagResult?> GetTagByIdAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the category identified by <paramref name="id"/> with the changes data defined in <paramref name="requestCategoryRequest"/>
        /// </summary>
        Task<CategoryResult> UpdateCategoryAsync(Guid id, UpdateCategoryRequest requestCategoryRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Updates the tag identified by <paramref name="id"/> with the changed data from <paramref name="updateTagRequest"/>
        /// </summary>
        Task<TagResult> UpdateTagAsync(Guid id, UpdateTagRequest updateTagRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the category identified by <paramref name="id"/>
        /// </summary>
        Task<bool> DeleteCategoryAsync(Guid id, bool recurse, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the category identified by <paramref name="parentId"/> and <paramref name="childName"/>
        /// </summary>
        Task<bool> DeleteCategoryAsync(Guid parentId, string childName, bool recurse, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves all Tags of the model
        /// </summary>
        Task<IEnumerable<TagResult>> GetTagsAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Copy the <paramref name="sourceCategoryId"/> as s subcategeory to <paramref name="destinationCategoryId"/>.
        /// It <paramref name="recurse"/> is true, all subcatageories and entites are clined as well.
        /// </summary>
        Task<CategoryResult> CopyCategoryToAsync(Guid sourceCategoryId, Guid destinationCategoryId, bool recurse, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the tag specified by <paramref name="id"/>.
        /// </summary>
        Task<bool> DeleteTagAsync(Guid id, CancellationToken cancellationToken);
    }
}