using Microsoft.Extensions.Logging;
using System;

namespace TreeStore.Model
{
    public partial class TreeStoreService
    {
        private readonly ILogger<TreeStoreService> logger;

        [LoggerMessage(10, LogLevel.Error, "Tag(id='{tagId}') wasn't updated: tag doesn't exist")]
        partial void LogUpdatingTagFailed(Guid tagId);

        [LoggerMessage(11, LogLevel.Error, "Tag(id='{tagId}') wasn't deleted: tag doesn't exist")]
        partial void LogDeletingTagFailed(Guid tagId);

        [LoggerMessage(20, LogLevel.Error, "Entity(id = '{entityId}') wasn't updated: entity doesn't exist")]
        partial void LogUpdatingEntityFailedEntityMissing(Guid entityId);

        [LoggerMessage(21, LogLevel.Error, "Entity(id='{entityId}') wasn't updated: duplicate name with Category(id='{categoryId}')")]
        partial void LogUpdatingEntityFailedFailedDuplicatName(Guid entityId, Guid categoryId);

        [LoggerMessage(22, LogLevel.Error, "Entity(id='{entityId}') wasn't deleted: entity doesn't exist")]
        partial void LogDeletingEntityFailed(Guid entityId);

        [LoggerMessage(30, LogLevel.Error, "Category(name='{categoryName}' wasn't created: Category(id='{parentId}') wasn't found")]
        partial void LogCreatingCategoryFailedMissingParent(string categoryName, Guid parentId);

        [LoggerMessage(31, LogLevel.Error, "Category(id='{categoryId}') wasn't deleted: category doesn't exist")]
        partial void LogDeletingCategoryFailed(Guid categoryId);

        [LoggerMessage(32, LogLevel.Error, "Category(name='{categoryName}',parentId'{parentId}') wasn't deleted: parent doesn't exist")]
        partial void LogDeletingCategoryByNameFailedMissingParent(Guid parentId, string categoryName);

        [LoggerMessage(33, LogLevel.Error, "Category(name='{categoryName}',parentId'{parentId}') wasn't deleted: child doesn't exist")]
        partial void LogDeletingCategoryByNameFailedMissingChild(Guid parentId, string categoryName);

        [LoggerMessage(34, LogLevel.Error, "Category(id='{categoryId}') wasn't read: it doesn't exist")]
        partial void LogReadingCategoryFailedMissing(Guid categoryId);

        [LoggerMessage(35, LogLevel.Error, "Category(id='{parentId}') children weren't read: it doesn't exist")]
        partial void LogReadingCategoryChildrenFailedMissingParent(Guid parentId);

        [LoggerMessage(36, LogLevel.Error, "Category(id='{categoryId}') wasn't updated: it doesn't exist")]
        partial void LogUpdatingCategoryFailedMissing(Guid categoryId);

        [LoggerMessage(37, LogLevel.Error, "Category(id='{categoryId}') wasn't updated: Entity(id='{entityId}') has same name: '{name}'")]
        partial void LogUpdatingCategoryFailedDuplicateName(Guid categoryId, string name, Guid entityId);

        [LoggerMessage(38, LogLevel.Information, "Category(id='{categoryId}',parentId='{parentId}',name='{categoryName}') was created")]
        partial void LogCategoryCreated(Guid categoryId, Guid parentId, string categoryName);
    }
}