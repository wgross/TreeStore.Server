using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TreeStore.Model;
using TreeStore.Model.Abstractions;

namespace TreeStore.LiteDb
{
    public class TreeStoreLiteDbPersistence : ITreeStoreModel
    {
        public LiteRepository LiteRepository { get; }

        public static TreeStoreLiteDbPersistence InMemory(ILoggerFactory loggerFactory) => new TreeStoreLiteDbPersistence(Options.Create(new TreeStoreLiteDbOptions()), loggerFactory);

        private readonly ILoggerFactory loggerFactory;
        private readonly TreeStoreLiteDbOptions options;

        public TreeStoreLiteDbPersistence(IOptions<TreeStoreLiteDbOptions> options, ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory;
            this.options = options.Value;
            this.LiteRepository = new(ConnectionString(this.options));
            // category repository is created once b/c it holds the root node as cached state.
            this.Categories = new CategoryLiteDbRepository(this, loggerFactory.CreateLogger<CategoryLiteDbRepository>());
        }

        private static string ConnectionString(TreeStoreLiteDbOptions options) => $"Filename={options.FileName}";

        #region ITreeStoreModel

        public ITagRepository Tags => new TagLiteDbRepository(LiteRepository, this.loggerFactory.CreateLogger<TagLiteDbRepository>());

        public ICategoryRepository Categories { get; }

        public IEntityRepository Entities => new EntityLiteDbRepository(this, this.loggerFactory.CreateLogger<EntityLiteDbRepository>());

        public IRelationshipRepository Relationships => new RelationshipLiteDbRepository(LiteRepository, this.loggerFactory.CreateLogger<RelationshipLiteDbRepository>());

        #endregion ITreeStoreModel

        #region IDisposable

        public void Dispose() => this.LiteRepository.Dispose();

        #endregion IDisposable
    }
}