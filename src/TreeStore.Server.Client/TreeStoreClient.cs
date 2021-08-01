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

        #region /entities

        /// <inheritdoc/>
        public async Task<EntityResult> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.PostAsJsonAsync("entities", createEntityRequest, cancellationToken).ConfigureAwait(false),
                convertToJson: async r => await r.Content.ReadFromJsonAsync<EntityResult>().ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EntityResult> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.GetAsync($"entities/{id}", cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<EntityResult>().ConfigureAwait(false)));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EntityResult>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.GetAsync("entities", cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<EntityCollectionResponse>().ConfigureAwait(false)).Entities).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EntityResult> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                 httpResponseMessage: await this.httpClient.PutAsJsonAsync($"entities/{id}", updateEntityRequest, cancellationToken).ConfigureAwait(false),
                 convertToJson: async r => (await r.Content.ReadFromJsonAsync<EntityResult>().ConfigureAwait(false))).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteEntityAsync(Guid id, CancellationToken cancellationToken)
        {
            var response = await this.HandleJsonResponse(
                 httpResponseMessage: await this.httpClient.DeleteAsync($"entities/{id}", cancellationToken).ConfigureAwait(false),
                 convertToJson: async r => (await r.Content.ReadFromJsonAsync<DeleteEntityResponse>().ConfigureAwait(false))).ConfigureAwait(false);

            return response.Deleted;
        }

        #endregion /entities

        #region /categories

        ///<inheritdoc/>
        public async Task<CategoryResult> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.PostAsJsonAsync("categories", request, cancellationToken).ConfigureAwait(false),
                convertToJson: async r => await r.Content.ReadFromJsonAsync<CategoryResult>().ConfigureAwait(false)).ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public async Task<CategoryResult> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.GetAsync($"categories/{id}", cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<CategoryResult>().ConfigureAwait(false)));
        }

        ///<inheritdoc/>
        public async Task<CategoryResult> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.PutAsJsonAsync($"categories/{id}", request, cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<CategoryResult>().ConfigureAwait(false))).ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public async Task<DeleteCategoryResponse> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.DeleteAsync($"categories/{id}", cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<DeleteCategoryResponse>().ConfigureAwait(false))).ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public async Task<CopyCategoryResponse> CopyCategoryToAsync(Guid sourceCategoryId, Guid destinationCategoryId, bool recurse, CancellationToken none)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient
                    .PostAsJsonAsync("categories/copy", new CopyCategoryRequest(
                        SourceId: sourceCategoryId,
                        DestinationId:
                        destinationCategoryId, Recurse: recurse)).ConfigureAwait(false),

                convertToJson: async r => (await r.Content.ReadFromJsonAsync<CopyCategoryResponse>().ConfigureAwait(false))).ConfigureAwait(false);
        }

        ///<inheritdoc/>
        public async Task<bool> DeleteCategoryAsync(Guid id, bool recurse, CancellationToken cancellationToken)
        {
            var response = await this.HandleJsonResponse(
                 httpResponseMessage: await this.httpClient.DeleteAsync($"categories/{id}?recurse={recurse}", cancellationToken).ConfigureAwait(false),
                 convertToJson: async r => (await r.Content.ReadFromJsonAsync<DeleteCategoryResponse>().ConfigureAwait(false))).ConfigureAwait(false);

            return response.Deleted;
        }

        #endregion /categories

        #region /tags

        /// <inheritdoc/>
        public async Task<TagResult> CreateTagAsync(CreateTagRequest createTagRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
               httpResponseMessage: await this.httpClient.PostAsJsonAsync("tags", createTagRequest, cancellationToken).ConfigureAwait(false),
               convertToJson: async r => await r.Content.ReadFromJsonAsync<TagResult>().ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<TagResult>> GetTagsAsync(CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.GetAsync("tags", cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<TagCollectionResponse>().ConfigureAwait(false)).Tags).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TagResult> GetTagByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                 httpResponseMessage: await this.httpClient.GetAsync($"tags/{id}", cancellationToken).ConfigureAwait(false),
                 convertToJson: async r => (await r.Content.ReadFromJsonAsync<TagResult>().ConfigureAwait(false)));
        }

        /// <inheritdoc/>
        public async Task<TagResult> UpdateTagAsync(Guid id, UpdateTagRequest updateTagRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.PutAsJsonAsync($"tags/{id}", updateTagRequest, cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<TagResult>().ConfigureAwait(false))).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteTagAsync(Guid id, CancellationToken cancellationToken)
        {
            var response = await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.DeleteAsync($"tags/{id}", cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<DeleteEntityResponse>().ConfigureAwait(false))).ConfigureAwait(false);

            return response.Deleted;
        }

        #endregion /tags

        #region Common Implementation

        private async Task<T> HandleJsonResponse<T>(HttpResponseMessage httpResponseMessage, Func<HttpResponseMessage, Task<T>> convertToJson)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return await convertToJson(httpResponseMessage);
            }
            else
            {
                switch (httpResponseMessage.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return default;

                    case HttpStatusCode.BadRequest:
                        throw await this.ThrowExceptionFromBadRequest(httpResponseMessage);
                    default:
                        throw new InvalidOperationException($"Unknown response status: {httpResponseMessage.StatusCode}");
                }
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

        private Exception CreateException(string exceptionName, string detail)
        {
            switch (exceptionName)
            {
                case nameof(InvalidOperationException):
                    return new InvalidOperationException(detail);

                case nameof(InvalidModelException):
                    return new InvalidModelException(detail);

                default:
                    return new ArgumentException(nameof(exceptionName));
            }
        }

        #endregion Common Implementation
    }
}