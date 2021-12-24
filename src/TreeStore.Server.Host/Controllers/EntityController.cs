using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStore.Server.Host.Controllers
{
    [ApiController]
    public class EntityController : ControllerBase
    {
        private readonly ITreeStoreService service;
        private readonly ILogger<EntityController> logger;

        public EntityController(ITreeStoreService service, ILogger<EntityController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [HttpPost, Route("entities")]
        public async Task<IActionResult> CreateEntityAsync(
            [FromBody] CreateEntityRequest request,
            CancellationToken cancellationToken)
        {
            var result = await this.service.CreateEntityAsync(request, cancellationToken);

            return this.CreatedAtAction("GetEntityById", new { id = result.Id }, result);
        }

        [HttpGet, Route("entities/{id}")]
        public async Task<IActionResult> GetEntityByIdAsync(
            [FromRoute(Name = "id")] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await this.service.GetEntityByIdAsync(id, cancellationToken);
            if (result is null)
                return this.NotFound();
            else
                return this.Ok(result);
        }

        [HttpGet, Route("entities")]
        public async Task<IActionResult> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            return this.Ok(new EntityCollectionResponse
            {
                Entities = (await this.service.GetEntitiesAsync(cancellationToken)).ToArray()
            });
        }

        [HttpPut, Route("entities/{id}")]
        public async Task<IActionResult> UpdateEntityAsync(
            [FromRoute(Name = "id")] Guid id,
            [FromBody] UpdateEntityRequest request,
            CancellationToken cancellationToken)
        {
            return this.Ok(await this.service.UpdateEntityAsync(id, request, cancellationToken));
        }

        [HttpDelete, Route("entities/{id}")]
        public async Task<IActionResult> DeleteEntityAsync(
            [FromRoute(Name = "id")] Guid id,
            CancellationToken cancellationToken)
        {
            return this.Ok(new DeleteEntityResponse(Deleted: await this.service.DeleteEntityAsync(id, cancellationToken)));
        }

        [HttpPost, Route("entities/copy")]
        public async Task<IActionResult> CopyEntityAsync(
            [FromBody] CopyEntityRequest request,
            CancellationToken cancellationToken)
        {
            return this.Ok(await this.service.CopyEntityToAsync(request.SourceId, request.DestinationId, cancellationToken).ConfigureAwait(false));
        }
    }
}