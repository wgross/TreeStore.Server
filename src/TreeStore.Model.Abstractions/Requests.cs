using System;

namespace TreeStore.Model.Abstractions
{
    public sealed record CreateEntityRequest(string Name, Guid CategoryId);

    public sealed record CreateTagRequest(string Name);

    public sealed record CreateCategoryRequest(string Name, Guid ParentId);

    public sealed record UpdateEntityRequest(string Name);

    public sealed record UpdateTagRequest(string Name);

    public sealed record UpdateCategoryRequest(string Name);

    public sealed record CopyCategoryRequest(Guid SourceId, Guid DestinationId, bool Recurse);
}