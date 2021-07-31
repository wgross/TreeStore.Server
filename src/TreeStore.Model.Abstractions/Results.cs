using System;

namespace TreeStore.Model.Abstractions
{
    public sealed record CategoryResult(Guid Id, string Name, Guid ParentId);
}