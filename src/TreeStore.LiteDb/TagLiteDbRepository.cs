using LiteDB;
using Microsoft.Extensions.Logging;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class TagLiteDbRepository : LiteDbRepositoryBase<Tag>, ITagRepository
    {
        public const string CollectionName = "tags";

        public TagLiteDbRepository(LiteRepository liteDbrepository, ILogger<TagLiteDbRepository> logger)
            : base(liteDbrepository, CollectionName, logger)
        {
            liteDbrepository.Database
                .GetCollection(CollectionName)
                .EnsureIndex(
                    name: nameof(Tag.Name),
                    expression: $"LOWER($.{nameof(Tag.Name)})",
                    unique: true);
        }

        protected override ILiteCollection<Tag> IncludeRelated(ILiteCollection<Tag> from) => from;

        public Tag FindByName(string name) => this.LiteCollection()
            .Query()
            .Where(t => t.Name.Equals(name))
            .FirstOrDefault();
    }
}