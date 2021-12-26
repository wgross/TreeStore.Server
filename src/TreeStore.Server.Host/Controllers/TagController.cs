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
    public class TagController : ControllerBase
    {
        private readonly ITreeStoreService service;
        private readonly ILogger<TagController> logger;

        public TagController(ITreeStoreService service, ILogger<TagController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [HttpPost, Route("tags")]
        public async Task<IActionResult> CreateTagAsync(
            [FromBody] CreateTagRequest request,
            CancellationToken cancellationToken)
        {
            var result = await this.service.CreateTagAsync(request, cancellationToken).ConfigureAwait(false);

            return this.CreatedAtAction("GetTagById", new { id = result.Id }, result);
        }

        [HttpGet, Route("tags/{id}")]
        public async Task<IActionResult> GetTagByIdAsync(
            [FromRoute(Name = "id")] Guid id,
            CancellationToken cancellationToken)
        {
            var result = await this.service.GetTagByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (result is null)
                return this.NotFound();
            else
                return this.Ok(result);
        }

        [HttpGet, Route("tags")]
        public async Task<IActionResult> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            return this.Ok(new TagCollectionResponse
            {
                Tags = (await this.service.GetTagsAsync(cancellationToken).ConfigureAwait(false)).ToArray()
            });
        }

        [HttpPut, Route("tags/{id}")]
        public async Task<IActionResult> UpdateTagAsync(
            [FromRoute(Name = "id")] Guid id,
            [FromBody] UpdateTagRequest request,
            CancellationToken cancellationToken)
        {
            return this.Ok(await this.service.UpdateTagAsync(id, request, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete, Route("tags/{id}")]
        public async Task<IActionResult> DeleteTagAsync(
            [FromRoute(Name = "id")] Guid id,
            CancellationToken cancellationToken)
        {
            return this.Ok(new DeleteTagResponse(Deleted: await this.service.DeleteTagAsync(id, cancellationToken).ConfigureAwait(false)));
        }
    }
}