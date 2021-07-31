using System;

namespace TreeStore.Model.Abstractions
{
    public sealed record EntityResult(Guid Id, string Name, Guid CategoryId);
}