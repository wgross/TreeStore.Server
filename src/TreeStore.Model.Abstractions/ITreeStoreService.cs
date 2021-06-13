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
        /// Updates the given properties at the given entity
        /// </summary>
        public Task<EntityResponse> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the entity having the <paramref name="id"/>
        /// </summary>
        public Task<DeleteEntityResponse> DeleteEntityAsync(Guid id, CancellationToken cancellationToken);
    }
}