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

        public static InvalidModelException EntityWithoutCategeory(Guid entityId, string name)
            => new InvalidModelException($"Entity(id='{entityId}',name='{name}') is mssing a category");

        public static InvalidModelException EntityWithDuplicateName(Guid entityId, Guid categoryId, Exception ex)
            => new InvalidModelException($"Entity(id='{entityId}') is a duplicate in category(id='{categoryId}')", ex);
    }
}