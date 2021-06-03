using LiteDB;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class TagRepository : LiteDbRepositoryBase<Tag>, ITagRepository
    {
        public const string CollectionName = "tags";

        public TagRepository(LiteRepository liteDbrepository) : base(liteDbrepository, CollectionName)
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