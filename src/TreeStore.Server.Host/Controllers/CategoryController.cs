using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStore.Server.Host.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ITreeStoreService service;

        public CategoryController(ITreeStoreService service)
        {
            this.service = service;
        }

        [HttpPost, Route("categories")]
        public async Task<IActionResult> CreateCategoryAsync([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var result = await this.service.CreateCategoryAsync(request, cancellationToken).ConfigureAwait(false);

            return this.CreatedAtAction("GetCategoryById", new { id = result.Id }, result);
        }

        [HttpGet, Route("categories")]
        public async Task<IActionResult> GetRootCategoryAsync(CancellationToken cancellationToken)
        {
            return this.Ok(await this.service.GetRootCategoryAsync(cancellationToken).ConfigureAwait(false));
        }

        [HttpGet, Route("categories/{id}")]
        public async Task<IActionResult> GetCategoryByIdAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken)
        {
            var result = await this.service.GetCategoryByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (result is null)
                return this.NotFound();
            else
                return this.Ok(result);
        }

        [HttpGet, Route("categories/{id}/children")]
        public async Task<IActionResult> GetCategoriesByIdAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken)
        {
            var result = await this.service.GetCategoriesByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (result is null)
                return this.NotFound();
            else
                return this.Ok(new CategoryCollectionResponse { Categories = result.ToArray() });
        }

        [HttpPut, Route("categories/{id}")]
        public async Task<IActionResult> UpdateEntityAsync([FromRoute(Name = "id")] Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            return this.Ok(await this.service.UpdateCategoryAsync(id, request, cancellationToken).ConfigureAwait(false));
        }

        [HttpDelete, Route("categories/{id}")]
        public async Task<IActionResult> DeleteCategoryAsync(
            [FromRoute(Name = "id")] Guid id,
            [FromQuery(Name = "recurse")] bool recurse,
            CancellationToken cancellationToken)
        {
            return this.Ok(new DeleteCategoryResponse(await this.service.DeleteCategoryAsync(id, recurse, cancellationToken).ConfigureAwait(false)));
        }

        [HttpDelete, Route("categories/{id}/{childName}")]
        public async Task<IActionResult> DeleteCategoryAsync(
            [FromRoute(Name = "id")] Guid id,
            [FromRoute(Name = "childName")] string childName,
            [FromQuery(Name = "recurse")] bool recurse,
            CancellationToken cancellationToken)
        {
            return this.Ok(new DeleteCategoryResponse(await this.service.DeleteCategoryAsync(id, childName, recurse, cancellationToken).ConfigureAwait(false)));
        }

        [HttpPost, Route("categories/copy")]
        public async Task<IActionResult> CopyCategoryAsync(
            [FromBody] CopyCategoryRequest request,
            CancellationToken cancellationToken)
        {
            return this.Ok(await this.service.CopyCategoryToAsync(request.SourceId, request.DestinationId, request.Recurse, cancellationToken).ConfigureAwait(false));
        }

        [HttpPost, Route("categories/move")]
        public async Task<IActionResult> MoveCategoryAsync(
            [FromBody] MoveCategoryRequest request,
            CancellationToken cancellationToken)
        {
            return this.Ok(await this.service.MoveCategoryToAsync(request.SourceId, request.DestinationId, cancellationToken).ConfigureAwait(false));
        }
    }
}