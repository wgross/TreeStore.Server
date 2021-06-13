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

        /// <inheritdoc/>
        public async Task<EntityResponse> CreateEntityAsync(CreateEntityRequest createEntityRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.PostAsJsonAsync("entities", createEntityRequest, cancellationToken).ConfigureAwait(false),
                convertToJson: async r => await r.Content.ReadFromJsonAsync<EntityResponse>().ConfigureAwait(false)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EntityResponse> GetEntityByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.GetAsync($"entities/{id}", cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<EntityResponse>().ConfigureAwait(false)));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<EntityResponse>> GetEntitiesAsync(CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                httpResponseMessage: await this.httpClient.GetAsync("entities", cancellationToken).ConfigureAwait(false),
                convertToJson: async r => (await r.Content.ReadFromJsonAsync<EntityResponseCollection>().ConfigureAwait(false)).Entities).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<EntityResponse> UpdateEntityAsync(Guid id, UpdateEntityRequest updateEntityRequest, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                 httpResponseMessage: await this.httpClient.PutAsJsonAsync($"entities/{id}", updateEntityRequest, cancellationToken).ConfigureAwait(false),
                 convertToJson: async r => (await r.Content.ReadFromJsonAsync<EntityResponse>().ConfigureAwait(false))).ConfigureAwait(false);
        }

        public async Task<DeleteEntityResponse> DeleteEntityAsync(Guid id, CancellationToken cancellationToken)
        {
            return await this.HandleJsonResponse(
                 httpResponseMessage: await this.httpClient.DeleteAsync($"entities/{id}", cancellationToken).ConfigureAwait(false),
                 convertToJson: async r => (await r.Content.ReadFromJsonAsync<DeleteEntityResponse>().ConfigureAwait(false))).ConfigureAwait(false);
        }

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

                default:
                    return new ArgumentException(nameof(exceptionName));
            }
        }
    }
}