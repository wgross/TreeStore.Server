using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStore.Server.Host.Controllers
{
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ITreeStoreService service;
        private readonly ILogger<CategoryController> logger;

        public CategoryController(ITreeStoreService service, ILogger<CategoryController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [HttpPost, Route("categories")]
        public async Task<IActionResult> CreateCategoryAsync([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var result = await this.service.CreateCategoryAsync(request, cancellationToken);

            return this.CreatedAtAction("GetCategoryById", new { id = result.Id }, result);
        }

        [HttpGet, Route("categories/{id}")]
        public async Task<IActionResult> GetCategoryByIdAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken)
        {
            var result = await this.service.GetCategoryByIdAsync(id, cancellationToken);
            if (result is null)
                return this.NotFound();
            else
                return this.Ok(result);
        }

        [HttpPut, Route("categories/{id}")]
        public async Task<IActionResult> UpdateEntityAsync([FromRoute(Name = "id")] Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            return this.Ok(await this.service.UpdateCategoryAsync(id, request, cancellationToken));
        }

        [HttpDelete, Route("categories/{id}")]
        public async Task<IActionResult> DeleteCategoryAsync([FromRoute(Name = "id")] Guid id, CancellationToken cancellationToken)
        {
            return this.Ok(await this.service.DeleteCategoryAsync(id, cancellationToken));
        }
    }
}