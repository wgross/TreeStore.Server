using System;
using System.Linq;

namespace TreeStore.Model.Abstractions
{
    #region Entity

    public record CreateEntityRequest(string Name, Guid CategoryId, CreateEntityTagsRequest? Tags = null, FacetPropertyValuesRequest? Values = null);

    public record UpdateEntityRequest(string? Name = null, UpdateEntityTagsRequest? Tags = null, FacetPropertyValuesRequest? Values = null);

    #region Entity/Tags

    public record CreateEntityTagsRequest
    {
        public CreateEntityTagsRequest()
        { }

        public CreateEntityTagsRequest(params AssignTagRequest[] creates)
        {
            this.Assigns = creates;
        }

        public AssignTagRequest[] Assigns { get; init; } = Array.Empty<AssignTagRequest>();
    }

    public record UpdateEntityTagsRequest
    {
        public UpdateEntityTagsRequest()
        { }

        public UpdateEntityTagsRequest(params EntityTagRequest[]? Requests)
        {
            var adds = Requests?.OfType<AssignTagRequest>().ToArray();
            if (adds!.Length > 0)
                this.Assigns = adds;

            var removes = Requests?.OfType<UnassignTagRequest>().ToArray();
            if (removes!.Length > 0)
                this.Unassigns = removes;
        }

        public AssignTagRequest[]? Assigns { get; init; } = Array.Empty<AssignTagRequest>();

        public UnassignTagRequest[]? Unassigns { get; init; } = Array.Empty<UnassignTagRequest>();
    }

    public record EntityTagRequest(Guid TagId);

    public record UnassignTagRequest(Guid TagId) : EntityTagRequest(TagId);

    public record AssignTagRequest(Guid TagId) : EntityTagRequest(TagId);

    #endregion Entity/Tags

    #region Entity/Values

    public record FacetPropertyValuesRequest
    {
        // required for JSON deserialization
        public FacetPropertyValuesRequest()
        { }

        public FacetPropertyValuesRequest(params UpdateFacetPropertyValueRequest[] requests)
        {
            Updates = requests.ToArray();
        }

        public UpdateFacetPropertyValueRequest[]? Updates { get; init; }
    }

    public record UpdateFacetPropertyValueRequest(Guid Id, FacetPropertyTypeValues Type, object? Value);

    #endregion Entity/Values

    #endregion Entity

    #region Tag

    public record CreateTagRequest(string Name, FacetRequest? Facet = null);

    public record UpdateTagRequest(string? Name = null, FacetRequest? Facet = null);

    public record FacetRequest
    {
        public FacetRequest(params FacetPropertyRequest[]? Properties)
        {
            var updates = Properties?.OfType<UpdateFacetPropertyRequest>().ToArray();
            if (updates!.Length > 0)
                this.Updates = updates;

            var creates = Properties?.OfType<CreateFacetPropertyRequest>().ToArray();
            if (creates!.Length > 0)
                this.Creates = creates;

            var deletes = Properties?.OfType<DeleteFacetPropertyRequest>().ToArray();
            if (deletes!.Length > 0)
                this.Deletes = deletes;
        }

        // required to deserialize from JSON
        public FacetRequest()
        { }

        public UpdateFacetPropertyRequest[]? Updates { get; init; } = Array.Empty<UpdateFacetPropertyRequest>();

        public CreateFacetPropertyRequest[]? Creates { get; init; } = Array.Empty<CreateFacetPropertyRequest>();

        public DeleteFacetPropertyRequest[]? Deletes { get; init; } = Array.Empty<DeleteFacetPropertyRequest>();
    }

    public record FacetPropertyRequest();

    public record FacetPropertyRequestId(Guid Id) : FacetPropertyRequest;

    public record DeleteFacetPropertyRequest(Guid Id) : FacetPropertyRequestId(Id);

    public record UpdateFacetPropertyRequest(Guid Id, string? Name = null) : FacetPropertyRequestId(Id);

    public record CreateFacetPropertyRequest(string Name, FacetPropertyTypeValues Type) : FacetPropertyRequest();

    #endregion Tag

    #region Category

    public record CreateCategoryRequest(string Name, Guid ParentId, FacetRequest? Facet = null);

    public record UpdateCategoryRequest(string? Name = null, FacetRequest? Facet = null);

    public record CopyCategoryRequest(Guid SourceId, Guid DestinationId, bool Recurse);

    public record CopyEntityRequest(Guid SourceId, Guid DestinationId);

    #endregion Category
}