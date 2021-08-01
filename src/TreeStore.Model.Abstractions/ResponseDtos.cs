using System;
using System.Collections.Generic;

namespace TreeStore.Model.Abstractions
{
    public sealed record DeleteCategoryResponse(bool Deleted);

    public sealed record DeleteEntityResponse(bool Deleted);

    public sealed record DeleteTagResponse(bool Deleted);

    public sealed record CopyCategoryResponse();

    public sealed record EntityCollectionResponse
    {
        public EntityResult[] Entities { get; init; } = Array.Empty<EntityResult>();
    }

    public sealed record CategoryCollectionResponse()
    {
        public CategoryResult[] Categories { get; init; } = Array.Empty<CategoryResult>();
    }

    public sealed record TagCollectionResponse()
    {
        public TagResult[] Tags { get; init; } = Array.Empty<TagResult>();

    }
}