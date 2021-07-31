using LiteDB;
using Microsoft.Extensions.Logging;
using System.IO;
using TreeStore.Model;
using TreeStore.Model.Abstractions;

namespace TreeStore.LiteDb
{
    public class TreeStoreLiteDbPersistence : ITreeStoreModel
    {
        public LiteRepository LiteRepository { get; }

        #region Create in Memory Storage

        public static TreeStoreLiteDbPersistence InMemory(ILoggerFactory loggerFactory) => new TreeStoreLiteDbPersistence(loggerFactory);

        private readonly ILoggerFactory loggerFactory;

        private TreeStoreLiteDbPersistence(ILoggerFactory loggerFactory)
            : this(new MemoryStream(), loggerFactory)
        {
        }

        private TreeStoreLiteDbPersistence(Stream storageStream, ILoggerFactory loggerFactory)
           : this(new LiteRepository(storageStream), loggerFactory)
        {
        }

        #endregion Create in Memory Storage

        #region Create File based Storage

        public static TreeStoreLiteDbPersistence InFile(string connectionString, ILoggerFactory loggerFactory)
            => new TreeStoreLiteDbPersistence(new LiteRepository(connectionString), loggerFactory);

        #endregion Create File based Storage

        private TreeStoreLiteDbPersistence(LiteRepository db, ILoggerFactory loggerFactory)

        {
            this.LiteRepository = db;
            this.loggerFactory = loggerFactory;

            // category repository is created once b/c it holds the root node as cached state.
            this.Categories = new CategoryLiteDbRepository(this, loggerFactory.CreateLogger<CategoryLiteDbRepository>());
        }

        public ITagRepository Tags => new TagLiteDbRepository(LiteRepository, this.loggerFactory.CreateLogger<TagLiteDbRepository>());

        public ICategoryRepository Categories { get; }

        public IEntityRepository Entities => new EntityLiteDbRepository(this, this.loggerFactory.CreateLogger<EntityLiteDbRepository>());

        public IRelationshipRepository Relationships => new RelationshipLiteDbRepository(LiteRepository, this.loggerFactory.CreateLogger<RelationshipLiteDbRepository>());

        public bool DeleteCategory(Category category, bool recurse)
        {
            var traverser = new CategoryRemovalTraverser((CategoryLiteDbRepository)this.Categories, (EntityLiteDbRepository)this.Entities);

            if (recurse)
                return traverser.DeleteRecursively(category);

            return traverser.DeleteIfEmpty(category);
        }

        public void CopyCategory(Category source, Category destination, bool recurse)
        {
            var traverser = new CategoryCopyTraverser((CategoryLiteDbRepository)this.Categories, (EntityLiteDbRepository)this.Entities);

            if (recurse)
                traverser.CopyCategoryRecursive(source, destination);
            else
                traverser.CopyCategory(source, destination);
        }

        public void Dispose()
        {
            this.LiteRepository.Dispose();
        }
    }
}