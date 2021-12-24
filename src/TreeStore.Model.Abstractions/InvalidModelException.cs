using System;

namespace TreeStore.Model.Abstractions
{
    public sealed class InvalidModelException : Exception
    {
        public InvalidModelException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        public InvalidModelException() : base()
        {
        }

        public InvalidModelException(string? message) : base(message)
        {
        }

        public static InvalidModelException EntityWithoutCategory(Guid entityId, string name)
            => new InvalidModelException($"Entity(id='{entityId}',name='{name}') is missing a category");

        public static InvalidModelException EntityWithDuplicateName(string entityName, Exception ex)
            => new InvalidModelException($"Can't write Entity(name='{entityName}'): duplicate name", ex);
    }
}