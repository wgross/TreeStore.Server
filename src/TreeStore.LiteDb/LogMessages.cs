using Microsoft.Extensions.Logging;
using System;

namespace TreeStore.LiteDb
{
    internal static partial class LogMessages
    {
        [LoggerMessage(1, LogLevel.Debug, "Deleting {type}(id='{id}')")]
        internal static partial void LogDeletingLiteDbItem(this ILogger logger, string type, Guid id);

        [LoggerMessage(2, LogLevel.Information, "Found existing root category(id='{id}')")]
        internal static partial void LogFoundExistingRootCategory(this ILogger logger, Guid id);

        [LoggerMessage(3, LogLevel.Information, "Created root category(id='{id}')")]
        internal static partial void LogCreatedNewRootCategory(this ILogger logger, Guid id);

        [LoggerMessage(4, LogLevel.Information, "Upserted {type}(id='{id}')")]
        internal static partial void LogUpsertedLiteDbItem(this ILogger logger, string type, Guid id);

        [LoggerMessage(5, LogLevel.Error, "Upserting {type}(id='{id}') failed in LitDb")]
        internal static partial void LogUpsertingLiteDbItemFailed(this ILogger logger, string type, Guid id);
    }
}