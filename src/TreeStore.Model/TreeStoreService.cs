using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task<CopyCategoryResponse> CopyCategoryToAsync(Guid sourceCategoryId, Guid destinationCategoryId, bool recurse, CancellationToken cancellationToken)
        {
            var sourceCategory = this.model.Categories.FindById(sourceCategoryId);
            if (sourceCategory is null)
                throw new InvalidOperationException($"Category(id='{sourceCategoryId}') wasn't copied: Category(id='{sourceCategoryId}') doesn't exist");

            var destinationCategory = this.model.Categories.FindById(destinationCategoryId);
            if (destinationCategory is null)
                throw new InvalidOperationException($"Category(id='{sourceCategoryId}') wasn't copied: Category(id='{destinationCategoryId}') doesn't exist");

            this.model.Categories.CopyTo(sourceCategory, destinationCategory, recurse);

            return Task.FromResult(new CopyCategoryResponse());
        }

        ///<inheritdoc/>
        public Task<CategoryResult> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var parent = this.model.Categories.FindById(request.ParentId);
            if (parent is null)
            {
                this.logger.LogError("Category(name='{categoryName}' wasn't created: Category(id='{parentId}') wasn't found", request.Name, request.ParentId);

                throw new InvalidOperationException($"Category(name='{request.Name}' wasn't created: Category(id='{request.ParentId}') wasn't found");
            }

            var category = new CategoryModel
            {
                Name = request.Name,
                Parent = parent
            };

            return Task.FromResult(this.model.Categories.Upsert(category).ToCategoryResult());
        }

        /// <Inheritdoc/>
        public Task<EntityResult> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
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

        ///<inheritdoc/>
        public Task<bool> DeleteEntityAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = this.model.Entities.FindById(id);
            if (entity is null)
            {
                this.logger.LogInformation("Entity(id='{entityId}') wasn't deleted: Entity(id='{entityId}') doesn't exist", id);

                return Task.FromResult(false);
            }
            return Task.FromResult(this.model.Entities.Delete(entity));
        }

        ///<inheritdoc/>
        public Task<CategoryResult?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.model.Categories.FindById(id)?.ToCategoryResult());
        }

        ///<inheritdoc/>
        public Task<IEnumerable<EntityResult>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        ///<inheritdoc/>
        public Task<EntityResult?> GetEntityByIdAsync(Guid id, CancellationToken cancelled)
        {
            return Task.FromResult(this.model.Entities.FindById(id)?.ToEntityResult());
        }

        /// <summary>
        /// Provides the root <see cref="CategoryModel"/> of this model.
        /// </summary>
        public CategoryModel GetRootCategory() => this.model.Categories.Root();

        ///<inheritdoc/>
        public Task<CategoryResult> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var category = this.model.Categories.FindById(id);
            if (category is null)
            {
                this.logger.LogError("Category(id='{categoryId}') wasn't updated: Category(id='{categoryId}') doesn't exist", id);

                throw new InvalidOperationException($"Category(id='{id}') wasn't updated: Category(id='{id}') doesn't exist");
            }

            request.Apply(category);

            return Task.FromResult(this.model.Categories.Upsert(category).ToCategoryResult());
        }

        ///<inheritdoc/>
        public Task<EntityResult> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken)
        {
            var entity = this.model.Entities.FindById(id);

            if (entity is null)
            {
                this.logger.LogError("Entity(id='{entityId}') wasn't updated: Entity(id='{entityId}') doesn't exist", id);

                throw new InvalidOperationException($"Entity(id='{id}') wasn't updated: Entity(id='{id}') doesn't exist");
            }

            updateEntityRequest.Apply(entity);

            return Task.FromResult(this.model.Entities.Upsert(entity).ToEntityResult());
        }

        ///<inheritdoc/>
        public Task<TagResult?> GetTagByIdAsync(Guid id, CancellationToken none)
        {
            return Task.FromResult(this.model.Tags.FindById(id)?.ToTagResult());
        }

        ///<inheritdoc/>
        public Task<TagResult> UpdateTagAsync(Guid id, UpdateTagRequest updateTagRequest, CancellationToken none)
        {
            var tag = this.model.Tags.FindById(id);

            if (tag is null)
            {
                this.logger.LogError("Tag(id='{tagId}') wasn't updated: Tag(id='{tagId}') doesn't exist", id);

                throw new InvalidOperationException($"Tag(id='{id}') wasn't updated: Tag(id='{id}') doesn't exist");
            }

            updateTagRequest.Apply(tag);

            this.model.Tags.Upsert(tag);

            return Task.FromResult(tag.ToTagResult());
        }

        ///<inheritdoc/>
        public Task<bool> DeleteTagAsync(Guid id, CancellationToken none)
        {
            var tag = this.model.Tags.FindById(id);
            if (tag is null)
            {
                this.logger.LogError("Tag(id='{tagId}') wasn't deleted: Tag(id='{tagId}') doesn't exist", id);

                return Task.FromResult(false);
            }

            return Task.FromResult(this.model.Tags.Delete(tag));
        }

        /// <inheritdoc/>
        public Task<TagResult> CreateTagAsync(CreateTagRequest createTagRequest, CancellationToken cancellationToken)
        {
            if (createTagRequest is null)
                throw new ArgumentNullException(nameof(createTagRequest));

            return Task.FromResult(this.model.Tags.Upsert(createTagRequest!.Apply()).ToTagResult());
        }

        /// <inheritdoc/>
        public Task<IEnumerable<TagResult>> GetTagsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.model.Tags.FindAll().Select(t => t.ToTagResult()));
        }
    }
}