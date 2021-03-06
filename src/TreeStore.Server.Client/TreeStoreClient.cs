using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;
using TreeStore.Model.Abstractions.Json;

namespace TreeStore.Server.Client
{
    public class TreeStoreClient : ITreeStoreService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<TreeStoreClient> logger;

        public TreeStoreClient(HttpClient httpClient, ILogger<TreeStoreClient> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        #region Entities /CREATE /COPY /MOVE

        /// <inheritdoc/>
        public async Task<EntityResult?> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<EntityResult>(
                httpResponseMessage: await this.httpClient.PostAsJsonAsync("entities", createEntityRequest, TreeStoreJsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EntityResult> CopyEntityToAsync(Guid sourceEntityId, Guid destinationCategoryId, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<EntityResult>(
                httpResponseMessage: await this.httpClient
                    .PostAsJsonAsync("entities/copy", new CopyEntityRequest(
                            SourceId: sourceEntityId,
                            DestinationId: destinationCategoryId),
                        TreeStoreJsonSerializerOptions.Default,
                        cancellationToken).ConfigureAwait(false),
                    cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EntityResult> MoveEntityToAsync(Guid sourceEntityId, Guid destinationCategoryId, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<EntityResult>(
               httpResponseMessage: await this.httpClient
                   .PostAsJsonAsync("entities/move", new MoveEntityRequest(
                           SourceId: sourceEntityId,
                           DestinationId: destinationCategoryId),
                       TreeStoreJsonSerializerOptions.Default,
                       cancellationToken).ConfigureAwait(false),
                   cancellationToken).ConfigureAwait(false);
        }

        #endregion Entities /CREATE /COPY /MOVE

        #region Entities /READ

        /// <inheritdoc/>
        public async Task<EntityResult?> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<EntityResult>(
                httpResponseMessage: await this.httpClient.GetAsync($"entities/{id}", cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EntityResult>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            return
            (
                await this.HandleJsonResponse<EntityCollectionResponse>(
                    httpResponseMessage: await this.httpClient.GetAsync("entities", cancellationToken).ConfigureAwait(false),
                    cancellationToken: cancellationToken).ConfigureAwait(false)
            ).Entities;
        }

        #endregion Entities /READ

        #region Entities /UPDATE

        /// <inheritdoc/>
        public async Task<EntityResult?> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<EntityResult>(
                 httpResponseMessage: await this.httpClient.PutAsJsonAsync($"entities/{id}", updateEntityRequest, TreeStoreJsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false),
                 cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        #endregion Entities /UPDATE

        #region Entities /DELETE

        /// <inheritdoc/>
        public async Task<bool> DeleteEntityAsync(Guid id, CancellationToken cancellationToken)
        {
            var response = await this.HandleJsonResponse<DeleteEntityResponse>(
                 httpResponseMessage: await this.httpClient.DeleteAsync($"entities/{id}", cancellationToken).ConfigureAwait(false),
                 cancellationToken: cancellationToken
            ).ConfigureAwait(false);

            return response.Deleted;
        }

        #endregion Entities /DELETE

        #region Categories /CREATE /COPY /MOVE

        /// <inheritdoc/>
        public async Task<CategoryResult?> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<CategoryResult>(
                httpResponseMessage: await this.httpClient.PostAsJsonAsync("categories", request, TreeStoreJsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<CategoryResult?> CopyCategoryToAsync(Guid sourceCategoryId, Guid destinationCategoryId, bool recurse, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<CategoryResult>(
                httpResponseMessage: await this.httpClient
                    .PostAsJsonAsync("categories/copy", new CopyCategoryRequest(
                            SourceId: sourceCategoryId,
                            DestinationId: destinationCategoryId,
                            Recurse: recurse),
                        TreeStoreJsonSerializerOptions.Default,
                        cancellationToken).ConfigureAwait(false),
                    cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<CategoryResult> MoveCategoryToAsync(Guid sourceCategoryId, Guid destinationCategoryId, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<CategoryResult>(
                httpResponseMessage: await this.httpClient
                    .PostAsJsonAsync("categories/move", new MoveCategoryRequest(
                            SourceId: sourceCategoryId,
                            DestinationId: destinationCategoryId),
                        TreeStoreJsonSerializerOptions.Default,
                        cancellationToken).ConfigureAwait(false),
                    cancellationToken).ConfigureAwait(false);
        }

        #endregion Categories /CREATE /COPY /MOVE

        #region Categories /READ

        /// <inheritdoc/>
        public async Task<IEnumerable<CategoryResult>?> GetCategoriesByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var categories = await this.HandleJsonResponse<CategoryCollectionResponse>(
                httpResponseMessage: await this.httpClient.GetAsync($"categories/{id}/children", cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return categories?.Categories;
        }

        /// <inheritdoc/>
        public async Task<CategoryResult?> GetRootCategoryAsync(CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<CategoryResult>(
                httpResponseMessage: await this.httpClient.GetAsync("categories", cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<CategoryResult?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<CategoryResult>(
                httpResponseMessage: await this.httpClient.GetAsync($"categories/{id}", cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        #endregion Categories /READ

        #region Categories /UPDATE

        /// <inheritdoc/>
        public async Task<CategoryResult?> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<CategoryResult>(
                httpResponseMessage: await this.httpClient.PutAsJsonAsync($"categories/{id}", request, TreeStoreJsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        #endregion Categories /UPDATE

        #region Categories /DELETE

        /// <inheritdoc/>
        public async Task<bool> DeleteCategoryAsync(Guid id, bool recurse, CancellationToken cancellationToken)
        {
            var response = await this.HandleJsonResponse<DeleteCategoryResponse>(
                 httpResponseMessage: await this.httpClient.DeleteAsync($"categories/{id}?recurse={recurse}", cancellationToken).ConfigureAwait(false),
                 cancellationToken: cancellationToken).ConfigureAwait(false);

            return response.Deleted;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteCategoryAsync(Guid id, string childName, bool recurse, CancellationToken cancellationToken)
        {
            var response = await this.HandleJsonResponse<DeleteCategoryResponse>(
                 httpResponseMessage: await this.httpClient.DeleteAsync($"categories/{id}/{childName}?recurse={recurse}", cancellationToken).ConfigureAwait(false),
                 cancellationToken: cancellationToken).ConfigureAwait(false);

            return response.Deleted;
        }

        #endregion Categories /DELETE

        #region Tags /CREATE

        /// <inheritdoc/>
        public async Task<TagResult?> CreateTagAsync(CreateTagRequest createTagRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<TagResult>(
               httpResponseMessage: await this.httpClient.PostAsJsonAsync("tags", createTagRequest, TreeStoreJsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false),
               cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        #endregion Tags /CREATE

        #region Tags /READ

        /// <inheritdoc/>
        public async Task<IEnumerable<TagResult>> GetTagsAsync(CancellationToken cancellationToken)
        {
            var response = await this.HandleJsonResponse<TagCollectionResponse>(
                httpResponseMessage: await this.httpClient.GetAsync("tags", cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return response.Tags;
        }

        /// <inheritdoc/>
        public async Task<TagResult?> GetTagByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<TagResult>(
                 httpResponseMessage: await this.httpClient.GetAsync($"tags/{id}", cancellationToken).ConfigureAwait(false),
                 cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        #endregion Tags /READ

        #region Tags /UPDATE

        /// <inheritdoc/>
        public async Task<TagResult?> UpdateTagAsync(Guid id, UpdateTagRequest updateTagRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse<TagResult>(
                httpResponseMessage: await this.httpClient.PutAsJsonAsync($"tags/{id}", updateTagRequest, TreeStoreJsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        #endregion Tags /UPDATE

        #region Tags /DELETE

        /// <inheritdoc/>
        public async Task<bool> DeleteTagAsync(Guid id, CancellationToken cancellationToken)
        {
            var response = await this.HandleJsonResponse<DeleteTagResponse>(
                httpResponseMessage: await this.httpClient.DeleteAsync($"tags/{id}", cancellationToken).ConfigureAwait(false),
                cancellationToken: cancellationToken).ConfigureAwait(false);

            return response.Deleted;
        }

        #endregion Tags /DELETE

        #region Common Implementation

        private async Task<T?> HandleJsonResponse<T>(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return await httpResponseMessage.Content
                    .ReadFromJsonAsync<T>(options: TreeStoreJsonSerializerOptions.Default, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                return httpResponseMessage.StatusCode switch
                {
                    HttpStatusCode.NotFound => default,
                    HttpStatusCode.BadRequest => throw await this.ThrowExceptionFromBadRequest(httpResponseMessage).ConfigureAwait(false),
                    _ => throw new InvalidOperationException($"Unknown response status: {httpResponseMessage.StatusCode}"),
                };
            }
        }

        private async Task<Exception> ThrowExceptionFromBadRequest(HttpResponseMessage httpResponseMessage)
        {
            var problemDetails = await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>().ConfigureAwait(false);

            return problemDetails.Extensions["ErrorType"] switch
            {
                JsonElement errorType when errorType.ValueKind == JsonValueKind.String => this.CreateException(errorType.GetString(), problemDetails.Detail),

                _ => new InvalidOperationException("Exception type is missing")
            };
        }

        private Exception CreateException(string? exceptionName, string detail)
        {
            return exceptionName switch
            {
                nameof(InvalidOperationException) => new InvalidOperationException(detail),
                nameof(InvalidModelException) => new InvalidModelException(detail),
                _ => new ArgumentException($"Unknown exception type {exceptionName}"),
            };
        }

        #endregion Common Implementation
    }
}