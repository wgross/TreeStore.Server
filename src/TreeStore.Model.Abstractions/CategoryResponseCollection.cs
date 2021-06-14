using System;

namespace TreeStore.Model.Abstractions
{
    public sealed record CategoryResponseCollection()
    {
        public CategoryResponse[] Categories { get; init; } = Array.Empty<CategoryResponse>();
    }
}