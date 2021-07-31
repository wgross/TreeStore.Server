using System;

namespace TreeStore.Model.Abstractions
{
    public sealed record DeleteCategoryResponse(bool Deleted);

    public sealed record DeleteEntityResponse(bool Deleted);

    public sealed record CopyCategoryResponse();

    public sealed record EntityResponseCollection
    {
        public EntityResult[] Entities { get; init; } = Array.Empty<EntityResult>();
    }

    public sealed record CategoryResponseCollection()
    {
        public CategoryResult[] Categories { get; init; } = Array.Empty<CategoryResult>();
    }
}