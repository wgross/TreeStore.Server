using Microsoft.Extensions.Logging;
using TreeStore.Model;
using TreeStore.Model.Abstractions;

namespace TreeStore.LiteDb
{
    internal static class LogMessages
    {
        internal static void DeletingLiteDbItem<T>(this ILogger logger, T liteDbItem) where T : IIdentifiable
        {
            logger.LogDebug("Deleting liteDbItem(id='{id}',type='{type}')", liteDbItem.Id, nameof(T));
        }

        internal static void FoundExistingRootCategory(this ILogger logger, CategoryModel category)
        {
            logger.LogInformation("Found existing root category(id='{id}')", category.Id);
        }

        internal static void CreatedNewRootCatagory(this ILogger logger, CategoryModel category)
        {
            logger.LogInformation("Created new root category(id='{id}')", category.Id);
        }

        internal static void UpsertedLiteDbItem<T>(this ILogger logger, T liteDbItem) where T : IIdentifiable
        {
            logger.LogInformation("Upserted liteDbItem(id='{id}',type='{type}')", liteDbItem.Id, nameof(T));
        }

        internal static void UpsertedLiteDbItemFailed<T>(this ILogger logger, T liteDbItem) where T : IIdentifiable
        {
            logger.LogWarning("Upserted liteDbItem(id='{id}',type='{type}') failed. LiteDb returned 'false'", liteDbItem.Id, nameof(T));
        }
    }
}