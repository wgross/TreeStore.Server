using System;

namespace TreeStore.Model.Abstractions
{
    public interface IIdentifiable
    {
        /// <summary>
        /// Any model item has an identifier
        /// </summary>
        Guid Id { get; }
    }
}