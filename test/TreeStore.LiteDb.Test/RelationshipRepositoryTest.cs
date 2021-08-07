using LiteDB;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
using TreeStore.Model;
using Xunit;

namespace TreeStore.LiteDb.Test
{
    public class RelationshipRepositoryTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly TreeStoreLiteDbPersistence persistence;
        private readonly EntityLiteDbRepository entityRepository;
        private readonly TagLiteDbRepository tagRepository;
        private readonly CategoryLiteDbRepository categoryRepository;
        private readonly RelationshipLiteDbRepository relationshipRepository;
        private readonly ILiteCollection<BsonDocument> relationships;

        public RelationshipRepositoryTest()
        {
            this.persistence = TreeStoreLiteDbPersistence.InMemory(new NullLoggerFactory());
            this.entityRepository = new EntityLiteDbRepository(this.persistence, new NullLogger<EntityLiteDbRepository>());
            this.tagRepository = new TagLiteDbRepository(this.persistence.LiteRepository, new NullLogger<TagLiteDbRepository>());
            this.categoryRepository = new CategoryLiteDbRepository(this.persistence, new NullLogger<CategoryLiteDbRepository>());
            this.relationshipRepository = new RelationshipLiteDbRepository(this.persistence.LiteRepository, new NullLogger<RelationshipLiteDbRepository>());
            this.relationships = this.persistence.LiteRepository.Database.GetCollection("relationships");
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact(Skip = "ignore relationships")]
        public void RelationshipRepository_writes_relationship_with_entities()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new EntityModel("e1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("e2"));
            var relationship = new Relationship("r", entity1, entity2);

            // ACT

            this.relationshipRepository.Upsert(relationship);

            // ASSERT

            var readRelationship = this.relationships.FindAll().Single();

            Assert.NotNull(readRelationship);
            Assert.Equal(relationship.Id, readRelationship.AsDocument["_id"].AsGuid);
            Assert.Equal(relationship.From.Id, readRelationship["From"].AsDocument["$id"].AsGuid);
            Assert.Equal(EntityLiteDbRepository.CollectionName, readRelationship["From"].AsDocument["$ref"].AsString);
            Assert.Equal(relationship.To.Id, readRelationship["To"].AsDocument["$id"].AsGuid);
            Assert.Equal(EntityLiteDbRepository.CollectionName, readRelationship["To"].AsDocument["$ref"].AsString);
        }

        [Fact(Skip = "ignore relationships")]
        public void RelationshipRepository_reads_relationship_with_entities()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new EntityModel("e1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("e2"));
            var relationship = new Relationship("r", entity1, entity2);

            this.relationshipRepository.Upsert(relationship);

            // ACT

            var resultById = this.relationshipRepository.FindById(relationship.Id);
            var resultByAll = this.relationshipRepository.FindAll().Single();

            // ASSERT

            Assert.Equal(entity1, resultById.From);
            Assert.NotSame(entity1, resultById.From);
            Assert.Equal(entity2, resultById.To);
            Assert.NotSame(entity2, resultById.To);

            Assert.Equal(entity1, resultByAll.From);
            Assert.NotSame(entity1, resultByAll.From);
            Assert.Equal(entity2, resultByAll.To);
            Assert.NotSame(entity2, resultByAll.To);
        }

        [Fact(Skip = "ignore relationships")]
        public void RelationshipRepository_writes_relationship_with_tag()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new TagModel("tag", new FacetModel("facet", new FacetPropertyModel("prop"))));
            var entity1 = this.entityRepository.Upsert(new EntityModel("e1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("e2"));
            var relationship = new Relationship("r", entity1, entity2, tag);

            // ACT

            this.relationshipRepository.Upsert(relationship);

            // ASSERT

            var readRelationship = this.relationships.FindAll().Single();

            Assert.NotNull(readRelationship);
            Assert.Equal(relationship.Id, readRelationship.AsDocument["_id"].AsGuid);
            Assert.Equal(relationship.Tags.Single().Id, readRelationship["Tags"].AsArray[0].AsDocument["$id"].AsGuid);
            Assert.Equal(TagLiteDbRepository.CollectionName, readRelationship["Tags"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact(Skip = "ignore relationships")]
        public void RelationshipRepository_reads_relationship_with_tag()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new TagModel("tag", new FacetModel("facet", new FacetPropertyModel("prop"))));
            var entity1 = this.entityRepository.Upsert(new EntityModel("e1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("e2"));
            var relationship = new Relationship("r", entity1, entity2, tag);

            this.relationshipRepository.Upsert(relationship);

            // ACT

            var resultById = this.relationshipRepository.FindById(relationship.Id);
            var resultByAll = this.relationshipRepository.FindAll().Single();

            // ASSERT

            Assert.Equal(tag, resultById.Tags.Single());
            Assert.NotSame(tag, resultById.Tags.Single());
            Assert.Equal(tag, resultByAll.Tags.Single());
            Assert.NotSame(tag, resultByAll.Tags.Single());
        }

        [Fact(Skip = "ignore relationships")]
        public void RelationshipRepository_writes_and_reads_Relationship_with_FacetProperty_values()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new TagModel("tag", new FacetModel("facet", new FacetPropertyModel("prop"))));
            var entity1 = this.entityRepository.Upsert(new EntityModel("e1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("e2"));
            var relationship = new Relationship("r", entity1, entity2, tag);

            // set facet property value
            relationship.SetFacetProperty(tag.Facet.Properties.Single(), 1);

            this.relationshipRepository.Upsert(relationship);

            // ACT

            var result = this.relationshipRepository.FindById(relationship.Id);

            // ASSERT

            //var comp = relationship.DeepCompare(result);

            //Assert.True(comp.AreEqual);
            //Assert.Equal(1, result.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2);
        }

        [Fact(Skip = "ignore relationships")]
        public void RelationshipRepository_removes_relationship()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new EntityModel("e1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("e2"));
            var relationship = new Relationship("r", entity1, entity2);

            this.relationshipRepository.Upsert(relationship);

            // ACT

            var result = this.relationshipRepository.Delete(relationship);

            // ASSERT

            Assert.True(result);
            Assert.Throws<InvalidOperationException>(() => this.relationshipRepository.FindById(relationship.Id));
        }

        [Fact(Skip = "ignore relationships")]
        public void RelationshipRepository_removing_unknown_relationhip_returns_false()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new EntityModel("e1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("e2"));
            var relationship = new Relationship("r", entity1, entity2);

            // ACT

            var result = this.relationshipRepository.Delete(relationship);

            // ASSERT

            Assert.False(result);
        }

        [Fact(Skip = "ignore relationships")]
        public void RelationshipRepository_finds_relationship_by_entity()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new EntityModel("e1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("e2"));
            var relationship1 = new Relationship("r", entity1, entity2);
            var relationship2 = new Relationship("r", entity2, entity1);

            this.relationshipRepository.Upsert(relationship1);

            this.relationshipRepository.Upsert(relationship2);

            // ACT

            var result = this.relationshipRepository.FindByEntity(entity1).ToArray();

            // ASSERT

            Assert.All(new[] { relationship1, relationship2 }, r => Assert.Contains(r, result));
        }

        [Fact(Skip = "ignore relationships")]
        public void RelationshipRepository_removes_mutiple_relationships()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new EntityModel("e1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("e2"));
            var relationship1 = new Relationship("r1", entity1, entity2);
            var relationship2 = new Relationship("r2", entity1, entity2);

            this.relationshipRepository.Upsert(relationship1);
            this.relationshipRepository.Upsert(relationship2);

            // ACT

            this.relationshipRepository.Delete(new[] { relationship1, relationship2 });

            // ASSERT

            Assert.Throws<InvalidOperationException>(() => this.relationshipRepository.FindById(relationship1.Id));
            Assert.Throws<InvalidOperationException>(() => this.relationshipRepository.FindById(relationship2.Id));
        }

        [Fact(Skip = "ignore relationships")]
        public void EntityRepositiry_finds_relationships_by_tag()
        {
            // ARRANGE
            var tag1 = this.tagRepository.Upsert(new TagModel("t1"));
            var tag2 = this.tagRepository.Upsert(new TagModel("t2"));
            var entity1 = this.entityRepository.Upsert(new EntityModel("entity1"));
            var entity2 = this.entityRepository.Upsert(new EntityModel("entity2"));
            var entity3 = this.entityRepository.Upsert(new EntityModel("entity3"));
            var entity4 = this.entityRepository.Upsert(new EntityModel("entity4"));
            var relationship1 = this.relationshipRepository.Upsert(new Relationship("r1", entity1, entity2, tag1));
            var relationship2 = this.relationshipRepository.Upsert(new Relationship("r1", entity3, entity4, tag2));

            // ACT

            var result = this.relationshipRepository.FindByTag(tag1).ToArray();

            // ASSERT

            Assert.Single(result);
            Assert.Equal(relationship1, result.Single());
        }
    }
}