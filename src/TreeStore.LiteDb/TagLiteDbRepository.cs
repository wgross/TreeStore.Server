using LiteDB;
using Microsoft.Extensions.Logging;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class TagLiteDbRepository : LiteDbRepositoryBase<TagModel>, ITagRepository
    {
        public TagLiteDbRepository(LiteRepository liteDbrepository, ILogger<TagLiteDbRepository> logger)
            : base(liteDbrepository, "tags", logger)
        {
            liteDbrepository.Database
                .GetCollection(this.CollectionName)
                .EnsureIndex(
                    name: nameof(TagModel.Name),
                    expression: $"LOWER($.{nameof(TagModel.Name)})",
                    unique: true);
        }

        protected override ILiteCollection<TagModel> IncludeRelated(ILiteCollection<TagModel> from) => from;

        public TagModel FindByName(string name) => this.LiteCollection()
            .Query()
            .Where(t => t.Name.Equals(name))
            .FirstOrDefault();
    }
}