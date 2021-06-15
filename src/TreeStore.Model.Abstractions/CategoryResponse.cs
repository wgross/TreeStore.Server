using System;

namespace TreeStore.Model.Abstractions
{
    public sealed record CategoryResponse(Guid Id, string Name, Guid ParentId);
}