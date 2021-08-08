using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public sealed partial class TreeStoreService
    {
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

        /// <inheritdoc/>
        public Task<EntityResult> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken)
        {
            var entity = this.Apply(createEntityRequest, new EntityModel());

            return Task.FromResult(this.model.Entities.Upsert(entity).ToEntityResult());
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

        ///<inheritdoc/>
        public Task<EntityResult> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken)
        {
            var entity = this.model.Entities.FindById(id);

            if (entity is null)
            {
                this.logger.LogError("Entity(id='{entityId}') wasn't updated: Entity(id='{entityId}') doesn't exist", id);

                throw new InvalidOperationException($"Entity(id='{id}') wasn't updated: Entity(id='{id}') doesn't exist");
            }

            this.Apply(updateEntityRequest, entity);

            return Task.FromResult(this.model.Entities.Upsert(entity).ToEntityResult());
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
            createEntityTagsRequest?.Assigns?.ForEach(creation =>
            {
                // throws is tag is null
                entityModel.AddTag(this.model.Tags.FindById(creation.TagId));
            });
        }

        private EntityModel Apply(UpdateEntityRequest updateEntityRequest, EntityModel entity)
        {
            entity.Name = updateEntityRequest.Name ?? entity.Name;

            this.Apply(updateEntityRequest.Tags, entity);

            return entity;
        }

        private void Apply(UpdateEntityTagsRequest? updateEntityTagsRequest, EntityModel entityModel)
        {
            updateEntityTagsRequest?.Assigns?.ForEach(creation =>
            {
                // throws is tag is null
                entityModel.AddTag(this.model.Tags.FindById(creation.TagId));
            });

            updateEntityTagsRequest?.Unassigns?.ForEach(removal =>
            {
                // throws is tag is null
                entityModel.RemoveTag(removal.TagId);
            });
        }
    }
}