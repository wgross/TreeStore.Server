using System;
using System.Linq;

namespace TreeStore.Model.Abstractions
{
    public record CreateEntityRequest(string Name, Guid CategoryId);

    public record CreateTagRequest(string Name, FacetRequest? Facet = null);

    public record CreateCategoryRequest(string Name, Guid ParentId);

    public record UpdateEntityRequest(string Name);

    public record UpdateTagRequest(string? Name = null, FacetRequest? Facet = null);

    public record FacetRequest
    {
        public FacetRequest(params FacetPropertyRequest[]? Properties)
        {
            var updates = Properties?.OfType<UpdateFacetPropertyRequest>().ToArray();
            if (updates!.Any())
                this.UpdateProperties = updates;

            var creates = Properties?.OfType<CreateFacetPropertyRequest>().ToArray();
            if (creates!.Any())
                this.CreateProperties = creates;

            var deletes = Properties?.OfType<DeleteFacetPropertyRequest>().ToArray();
            if (deletes!.Any())
                this.DeleteProperties = deletes;
        }

        public FacetRequest()
        {
        }

        public UpdateFacetPropertyRequest[]? UpdateProperties { get; init; } = Array.Empty<UpdateFacetPropertyRequest>();

        public CreateFacetPropertyRequest[]? CreateProperties { get; init; } = Array.Empty<CreateFacetPropertyRequest>();

        public DeleteFacetPropertyRequest[]? DeleteProperties { get; init; } = Array.Empty<DeleteFacetPropertyRequest>();
    }

    public record FacetPropertyRequest();

    public record FacetPropertyRequestId(Guid Id) : FacetPropertyRequest;

    public record DeleteFacetPropertyRequest(Guid Id) : FacetPropertyRequestId(Id);

    public record UpdateFacetPropertyRequest(Guid Id, string? Name = null) : FacetPropertyRequestId(Id);

    public record CreateFacetPropertyRequest(string Name, FacetPropertyTypeValues Type) : FacetPropertyRequest();

    public record UpdateCategoryRequest(string Name);

    public record CopyCategoryRequest(Guid SourceId, Guid DestinationId, bool Recurse);
}