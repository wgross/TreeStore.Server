using LiteDB;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using TreeStore.Model;
using TreeStore.Model.Abstractions;

namespace TreeStore.LiteDb
{
    public class EntityLiteDbRepository : LiteDbRepositoryBase<EntityModel>, IEntityRepository
    {
        public const string CollectionName = "entities";

        static EntityLiteDbRepository()
        {
            BsonMapper.Global
                .Entity<EntityModel>()
                    .DbRef(e => e.Tags, TagLiteDbRepository.CollectionName)
                    .DbRef(e => e.Category, CategoryLiteDbRepository.collectionName);
        }

        public EntityLiteDbRepository(TreeStoreLiteDbPersistence persistence, ILogger<EntityLiteDbRepository> logger)
            : base(persistence.LiteRepository, CollectionName, logger)
        {
            // name+categoryid of an entity is unique
            persistence
                .LiteRepository
                .Database
                .GetCollection(CollectionName)
                .EnsureIndex(
                    name: nameof(EntityModel.UniqueName),
                    expression: $"$.{nameof(EntityModel.UniqueName)}",
                    unique: true);

            // retrieve entities by category id
            persistence
                .LiteRepository
                .Database
                .GetCollection<EntityModel>(CollectionName)
                .EnsureIndex(e => e.Category);
        }

        protected override ILiteCollection<EntityModel> IncludeRelated(ILiteCollection<EntityModel> from) => from.Include(e => e.Tags);

        protected ILiteQueryable<EntityModel> QueryRelated() => this.LiteCollection().Query().Include(e => e.Tags).Include(e => e.Category);

        public override EntityModel Upsert(EntityModel entity)
        {
            if (entity.Category is null)
                throw InvalidModelException.EntityWithoutCategory(entity.Id, entity.Name);

            try
            {
                return base.Upsert(entity);
            }
            catch (LiteException ex)
            {
                throw InvalidModelException.EntityWithDuplicateName(entity.Id, entity.Category.Id, ex);
            }
        }

        public override bool Delete(EntityModel entity)
        {
            var relationshipExists = this.LiteRepository
                .Query<Relationship>("relationships")
                .Include(r => r.From)
                .Include(r => r.To)
                .Where(r => r.From!.Id.Equals(entity.Id) || r.To!.Id.Equals(entity.Id))
                .Exists();

            if (relationshipExists)
                return false;

            if (base.Delete(entity))
            {
                return true;
            }
            return false;
        }

        public IEnumerable<EntityModel> FindByTag(TagModel tag) => this.QueryRelated()
            // todo: optimize
            // i'm sure this is a table scan...LiteDb 5 may index that?
            .Where(e => e.Tags.Contains(tag))
            .ToArray();

        public IEnumerable<EntityModel> FindByCategory(CategoryModel category) => this.QueryRelated()
            .Where(e => e.Category!.Id == category.Id)
            .ToArray();

        //public IEnumerable<Entity> FindByCategory(Category category) => this.QueryRelated()
        //    .Where(e => e.Category != null && e.Category.Id == category.Id)
        //    .ToArray();

        public EntityModel? FindByCategoryAndName(CategoryModel category, string name) => this.QueryRelated()
            .Where(e => e.Category!.Id == category.Id && e.Name.Equals(name))
            .FirstOrDefault();
    }
}