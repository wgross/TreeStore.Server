using System;

namespace TreeStore.Model.Abstractions
{
    public sealed record CreateCategoryRequest(string Name, Guid ParentId);
}