﻿using LiteDB;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class RelationshipRepository : LiteDbRepositoryBase<Relationship>, IRelationshipRepository
    {
        static RelationshipRepository()
        {
            BsonMapper.Global
               .Entity<Relationship>()
                   .DbRef(r => r.Tags, TagRepository.CollectionName)
                   .DbRef(r => r.From, "entities")
                   .DbRef(r => r.To, "entities");
        }

        public RelationshipRepository(LiteRepository repo) : base(repo, "relationships")
        {
        }

        protected override ILiteCollection<Relationship> IncludeRelated(ILiteCollection<Relationship> from) => from
            .Include(r => r.From)
            .Include(r => r.To)
            .Include(r => r.Tags);

        public override Relationship Upsert(Relationship relationship)
        {
            return relationship;
        }

        public override bool Delete(Relationship relationship)
        {
            if (base.Delete(relationship))
            {
                return true;
            }
            return false;
        }

        public void Delete(IEnumerable<Relationship> relationships)
            => relationships.ToList().ForEach(r => this.Delete(r));

        public IEnumerable<Relationship> FindByEntity(Entity entity) => this.IncludeRelated(this.LiteCollection())
            .Query()
            .Where(r => r.From!.Id.Equals(entity.Id) || r.To!.Id.Equals(entity.Id))
            .ToEnumerable();

        public IEnumerable<Relationship> FindByTag(Tag tag) => this.IncludeRelated(this.LiteCollection())
            .Query()
            // todo: optimize, needs index?
            .Where(r => r.Tags.Contains(tag))
            .ToEnumerable();
    }
}