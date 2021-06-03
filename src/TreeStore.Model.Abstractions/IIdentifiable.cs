using System;

namespace TreeStore.Model.Abstractions
{
    public interface IIdentifiable
    {
        Guid Id { get; }
    }
}