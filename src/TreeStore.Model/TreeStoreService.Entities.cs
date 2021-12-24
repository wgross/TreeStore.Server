using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public sealed partial class TreeStoreService
    {
        /// <inheritdoc/>
        public Task<bool> DeleteEntityAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = this.model.Entities.FindById(id);
            if (entity is null)
            {
                this.LogDeletingEntityFailed(id);

                return Task.FromResult(false);
            }
            return Task.FromResult(this.model.Entities.Delete(entity));
        }

        /// <inheritdoc/>
        public Task<EntityResult> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken)
        {
            var entity = this.Apply(createEntityRequest, new EntityModel());

            return Task.FromResult(this.model.Entities.Upsert(entity).ToEntityResult());
        }

        /// <inheritdoc/>
        public Task<IEnumerable<EntityResult>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<EntityResult?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.model.Entities.FindById(id)?.ToEntityResult());
        }

        /// <inheritdoc/>
        public Task<EntityResult> UpdateEntityAsync(Guid id, UpdateEntityRequest request, CancellationToken cancellationToken)
        {
            var entity = this.model.Entities.FindById(id);

            if (entity is null)
            {
                this.LogUpdatingEntityFailedEntityMissing(id);

                throw new InvalidOperationException($"Entity(id='{id}') wasn't updated: Entity(id='{id}') doesn't exist");
            }

            if (request.Name is not null)
            {
                // name is about to be updated.
                // check if there is an entity having the same name
                var category = this.model.Categories.FindByParentAndName(entity.Category!, request.Name);
                if (category is not null)
                {
                    this.LogUpdatingEntityFailedFailedDuplicatName(entity.Id, category.Id);

                    throw new InvalidOperationException($"Entity(id='{entity.Id}') wasn't updated: duplicate name with Category(id='{category.Id}')");
                }
            }

            this.Apply(request, entity);

            return Task.FromResult(this.model.Entities.Upsert(entity).ToEntityResult());
        }

        /// <inheritdoc/>
        public async Task<EntityResult> CopyEntityToAsync(Guid entityId, Guid destinationId, CancellationToken none)
        {
            var entity = this.model.Entities.FindById(entityId);
            if (entity is null)
                throw new InvalidOperationException($"Entity(id='{entityId}') wasn't copied: it doesn't exist");

            var destinationCategory = this.model.Categories.FindById(destinationId);
            if (destinationCategory is null)
                throw new InvalidOperationException($"Entity(id='{entityId}') wasn't copied: Category(id='{destinationId}') doesn't exist");

            var existingDuplicate = this.model.Categories.FindByParentAndName(destinationCategory, entity.Name);
            if (existingDuplicate is not null)
                throw new InvalidOperationException($"Entity(id='{entity.Id}') wasn't copied: name is duplicate of Category(id='{existingDuplicate.Id}')");

            return this.model.Categories.CopyTo(entity, destinationCategory).ToEntityResult();
        }

        private EntityModel Apply(CreateEntityRequest createEntityRequest, EntityModel entityModel)
        {
            entityModel.Name = createEntityRequest.Name;
            entityModel.Category = this.model.Categories.FindById(createEntityRequest.CategoryId);

            this.Apply(createEntityRequest.Tags, entityModel);

            return entityModel;
        }

        private void Apply(CreateEntityTagsRequest? createEntityTagsRequest, EntityModel entityModel)
        {
            createEntityTagsRequest?.Assigns?.ForEach(creation => entityModel.AddTag(this.model.Tags.FindById(creation.TagId)));
        }

        private EntityModel Apply(UpdateEntityRequest updateEntityRequest, EntityModel entity)
        {
            entity.Name = updateEntityRequest.Name ?? entity.Name;

            this.Apply(updateEntityRequest.Tags, entity);
            Apply(updateEntityRequest.Values, entity);

            return entity;
        }

        private static void Apply(FacetPropertyValuesRequest? values, EntityModel entity)
        {
            if (values is null)
                return;

            var facetProperties = entity.FacetProperties().ToDictionary(fp => fp.Id);

            if (values.Updates is null)
                return;

            foreach (var value in values.Updates)
            {
                if (facetProperties.TryGetValue(value.Id, out var facetProperty))
                {
                    entity.SetFacetProperty(facetProperty, value.Value);
                }
            }
        }

        private void Apply(UpdateEntityTagsRequest? updateEntityTagsRequest, EntityModel entityModel)
        {
            updateEntityTagsRequest?.Assigns?.ForEach(creation => entityModel.AddTag(this.model.Tags.FindById(creation.TagId)));

            updateEntityTagsRequest?.Unassigns?.ForEach(removal => entityModel.RemoveTag(removal.TagId));
        }
    }
}