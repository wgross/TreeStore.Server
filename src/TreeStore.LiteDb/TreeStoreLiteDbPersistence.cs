using LiteDB;
using System.IO;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class TreeStoreLiteDbPersistence : ITreeStorePersistence
    {
        private LiteRepository db;

        #region Create in Memory Storage

        public static TreeStoreLiteDbPersistence InMemory() => new TreeStoreLiteDbPersistence();

        private TreeStoreLiteDbPersistence()
            : this(new MemoryStream())
        {
        }

        private TreeStoreLiteDbPersistence(Stream storageStream)
           : this(new LiteRepository(storageStream))
        {
        }

        #endregion Create in Memory Storage

        #region Create File based Storage

        public static TreeStoreLiteDbPersistence InFile(string connectionString)
            => new TreeStoreLiteDbPersistence( new LiteRepository(connectionString));

        #endregion Create File based Storage

        private TreeStoreLiteDbPersistence(LiteRepository db)

        {
            this.db = db;
            this.Categories = new CategoryLiteDbRepository(db);
        }

        public ITagRepository Tags => new TagRepository(db);

        public ICategoryRepository Categories { get; private set; }

        public IEntityRepository Entities => new EntityRepository(db);

        public IRelationshipRepository Relationships => new RelationshipRepository(db);

        public bool DeleteCategory(Category category, bool recurse)
        {
            var traverser = new CategoryRemovalTraverser((CategoryLiteDbRepository)this.Categories, (EntityRepository)this.Entities);

            if (recurse)
                return traverser.DeleteRecursively(category);

            return traverser.DeleteIfEmpty(category);
        }

        public void CopyCategory(Category source, Category destination, bool recurse)
        {
            var traverser = new CategoryCopyTraverser((CategoryLiteDbRepository)this.Categories, (EntityRepository)this.Entities);

            if (recurse)
                traverser.CopyCategoryRecursively(source, destination);
            else
                traverser.CopyCategory(source, destination);
        }

        public void Dispose()
        {
            this.db.Dispose();
        }
    }
}