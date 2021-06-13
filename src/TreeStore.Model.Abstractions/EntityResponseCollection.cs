using System;

namespace TreeStore.Model.Abstractions
{
    public sealed record EntityResponseCollection
    {
        public EntityResponse[] Entities { get; init; } = Array.Empty<EntityResponse>();
    }
}