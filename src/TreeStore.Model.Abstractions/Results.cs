using System;

namespace TreeStore.Model.Abstractions
{
    public sealed record CategoryResult(Guid Id, string Name, Guid ParentId);

    public sealed record TagResult(Guid Id, string Name);

    public sealed record EntityResult(Guid Id, string Name, Guid CategoryId);
}