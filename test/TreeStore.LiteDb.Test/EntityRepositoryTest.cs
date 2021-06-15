using LiteDB;
using System;
using System.Linq;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using Xunit;

namespace TreeStore.LiteDb.Test
{
    public class EntityRepositoryTest : LiteDbTestBase, IDisposable
    {
        private readonly RelationshipLiteDbRepository relationshipRepository;
        private readonly ILiteCollection<BsonDocument> entitiesCollection;

        public EntityRepositoryTest()
        {
            this.relationshipRepository = new RelationshipLiteDbRepository(this.LiteDb);
            this.entitiesCollection = this.LiteDb.Database.GetCollection("entities");
        }

        [Fact]
        public void EntityRepository_writes_entity()
        {
            // ARRANGE
            var entity = DefaultEntity(WithDefaultTag);

            // ACT
            this.EntityRepository.Upsert(entity);

            // ASSERT
            var readEntity = this.entitiesCollection.FindAll().Single();

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Name, readEntity.AsDocument["Name"].AsString);

            // unique identifier from category and root is stored (name-is-under-parent constraint)
            Assert.Equal($"{entity.Name.ToLowerInvariant()}_{this.CategoryRepository.Root().Id}", readEntity.AsDocument["UniqueName"].AsString);

            // entity is in root category (root -> entity.id)
            Assert.Equal(this.CategoryRepository.Root().Id, readEntity.AsDocument["Category"].AsDocument["$id"].AsGuid);
            Assert.Equal("categories", readEntity.AsDocument["Category"].AsDocument["$ref"].AsString);
            Assert.Equal("categories", readEntity.AsDocument["Category"].AsDocument["$ref"].AsString);

            // single tag
            Assert.Single(readEntity.AsDocument["Tags"].AsArray);
            Assert.Equal(entity.Tags.Single().Id, readEntity.BsonValue("Tags").AsArray.Single().AsDocument.BsonValue("$id").AsGuid);

            // no values
            Assert.Empty(readEntity.AsDocument["Values"].AsDocument);
        }

        [Fact]
        public void EntitiyRepository_writing_entity_rejects_duplicate_name_in_same_category()
        {
            // ARRANGE
            var entity = this.EntityRepository.Upsert(DefaultEntity());

            // ACT
            var secondEntity = DefaultEntity();
            var result = Assert.Throws<InvalidModelException>(() => this.EntityRepository.Upsert(secondEntity));

            // ASSERT
            // duplicate was rejected
            Assert.StartsWith($"Entity(id='", result.Message);
            Assert.EndsWith($"') is a duplicate in category(id='{entity.Category.Id}')", result.Message);
            Assert.IsType<LiteException>(result.InnerException);
            Assert.Equal($"Cannot insert duplicate key in unique index 'UniqueName'. The duplicate value is '\"e_{this.CategoryRepository.Root().Id}\"'.", result.InnerException.Message);
            Assert.Single(this.entitiesCollection.FindAll());
        }

        [Fact]
        public void EntitiyRepository_writing_entity_rejects_missing_category()
        {
            // ARRANGE
            var entity = DefaultEntity(WithoutCategory);

            // ACT
            var result = Assert.Throws<InvalidModelException>(() => this.EntityRepository.Upsert(entity));

            // ASSERT
            Assert.Equal($"Entity(id='{entity.Id}',name='{entity.Name}') is mssing a category", result.Message);
        }

        [Fact]
        public void EntitiyRepository_write_entity_with_duplicate_name_to_different_categories()
        {
            // ARRANGE
            this.EntityRepository.Upsert(DefaultEntity());
            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var secondEntity = DefaultEntity(WithEntityCategory(category));

            // ACT
            this.EntityRepository.Upsert(secondEntity);

            // ASSERT
            // notification was sent only once
            Assert.Equal(2, this.entitiesCollection.FindAll().Count());
        }

        [Fact]
        public void EntityRepository_reads_entity_by_id()
        {
            // ARRANGE
            var entity = this.EntityRepository.Upsert(DefaultEntity());

            // ACT
            var result = this.EntityRepository.FindById(entity.Id);

            // ASSERT
            Assert.Equal(entity, result);
            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Category.Id, result.Category.Id);
            Assert.Empty(result.Tags);
            Assert.Equal(entity.UniqueName, result.UniqueName);
            Assert.Empty(result.Values);
        }

        [Fact]
        public void EntityRepository_removes_entity()
        {
            // ARRANGE
            var entity = this.EntityRepository.Upsert(DefaultEntity());

            // ACT
            var result = this.EntityRepository.Delete(entity);

            // ASSERT
            Assert.True(result);
            Assert.Null(this.EntityRepository.FindById(entity.Id));
        }

        [Fact]
        public void EntityRepository_removing_unknown_entity_returns_false()
        {
            // ARRANGE
            var entity = DefaultEntity();

            // ACT
            var result = this.EntityRepository.Delete(entity);

            // ASSERT
            Assert.False(result);
        }

        [Fact(Skip = "ignore relationships")]
        public void EntityRepository_removing_entity_fails_if_used_in_relationship()
        {
            // ARRANGE
            var entity1 = this.EntityRepository.Upsert(DefaultEntity(e => e.Name = "entity1"));
            var entity2 = this.EntityRepository.Upsert(DefaultEntity(e => e.Name = "entity2"));

            this.relationshipRepository.Upsert(new Relationship("relationship1", entity1, entity2));

            // ACT
            var result = this.EntityRepository.Delete(entity1);

            // ASSERT
            Assert.False(result);
        }

        #region Entity -1:*-> Tag

        [Fact]
        public void EntityRepository_writes_entity_with_tag()
        {
            // ARRANGE
            var tag = this.TagRepository.Upsert(DefaultTag());
            var entity = DefaultEntity(e => e.AddTag(tag));

            // ACT
            this.EntityRepository.Upsert(entity);

            // ASSERT
            var readEntity = this.entitiesCollection.FindById(entity.Id);

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Tags.Single().Id, readEntity["Tags"].AsArray[0].AsDocument["$id"].AsGuid);
            Assert.Equal(TagLiteDbRepository.CollectionName, readEntity["Tags"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void EntityRepository_reads_entity_with_tag_by_Id()
        {
            // ARRANGE
            var tag = this.TagRepository.Upsert(DefaultTag(WithDefaultProperty));
            var entity = this.EntityRepository.Upsert(DefaultEntity(e => e.AddTag(tag)));

            // ACT
            var result = this.EntityRepository.FindById(entity.Id);

            // ASSERT
            Assert.Equal(entity, result);
            Assert.NotSame(entity, result);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Tags.Single().Id, result.Tags.Single().Id);
            Assert.Equal(entity.Tags.Single().Name, result.Tags.Single().Name);
            Assert.Equal(entity.Tags.Single().Facet.Name, result.Tags.Single().Facet.Name);
            Assert.Equal(entity.Tags.Single().Facet.Properties.Single().Name, result.Tags.Single().Facet.Properties.Single().Name);
        }

        [Fact]
        public void EntityRepository_reads_entity_with_tag_by_Name()
        {
            // ARRANGE
            var tag = this.TagRepository.Upsert(DefaultTag(WithDefaultProperty));
            var entity = this.EntityRepository.Upsert(DefaultEntity(WithoutTags, WithEntityCategory(this.CategoryRepository.Root()), e => e.AddTag(tag)));

            // ACT
            var result = this.EntityRepository.FindAll().Single();

            // ASSERT
            Assert.Equal(entity, result);
            Assert.NotSame(entity, result);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Tags.Single().Id, result.Tags.Single().Id);
            Assert.Equal(entity.Tags.Single().Name, result.Tags.Single().Name);
            Assert.Equal(entity.Tags.Single().Facet.Name, result.Tags.Single().Facet.Name);
            Assert.Equal(entity.Tags.Single().Facet.Properties.Single().Name, result.Tags.Single().Facet.Properties.Single().Name);
        }

        [Fact]
        public void EntityRepository_finds_entities_by_tag()
        {
            // ARRANGE
            var tag1 = this.TagRepository.Upsert(DefaultTag(t => t.Name = "t1"));
            var tag2 = this.TagRepository.Upsert(DefaultTag(t => t.Name = "t2"));

            var entity1 = this.EntityRepository.Upsert(DefaultEntity(e =>
            {
                e.Name = "entity1";
                e.AddTag(tag1);
            }));

            var entity2 = this.EntityRepository.Upsert(DefaultEntity(e =>
            {
                e.Name = "entity2";
                e.AddTag(tag2);
            }));

            // ACT
            var result = this.EntityRepository.FindByTag(tag1);

            // ASSERT
            Assert.Single(result);
            Assert.Equal(entity1, result.Single());
        }

        #endregion Entity -1:*-> Tag

        #region Entity -0:*-> PropertyValues

        [Fact]
        public void EntityRepository_writes_Entity_with_FacetProperty_values()
        {
            // ARRANGE
            var value = Guid.NewGuid();
            var tag = this.TagRepository.Upsert(DefaultTag(WithDefaultProperty));
            var entity = this.EntityRepository.Upsert(DefaultEntity(e =>
            {
                e.AddTag(tag);
                e.SetFacetProperty(tag.Facet.Properties.Single(), value);
            }));

            // ACT
            this.EntityRepository.Upsert(entity);

            // ASSERT
            var readEntity = this.entitiesCollection.FindById(entity.Id);

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(
                entity.Values[entity.Tags.Single().Facet.Properties.Single().Id.ToString()],
                readEntity["Values"].AsDocument[entity.Tags.Single().Facet.Properties.Single().Id.ToString()].AsGuid);
            Assert.Equal(TagLiteDbRepository.CollectionName, readEntity["Tags"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void EntityRepository_reads_Entity_with_FacetProperty_values()
        {
            // ARRANGE
            var value = Guid.NewGuid();
            var tag = this.TagRepository.Upsert(DefaultTag(WithDefaultProperty));
            var entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(this.CategoryRepository.Root()), WithoutTags, e =>
             {
                 e.AddTag(tag);
             }));

            // set facet property value
            entity.SetFacetProperty(entity.Tags.Single().Facet.Properties.Single(), value);

            this.EntityRepository.Upsert(entity);

            // ACT
            var result = this.EntityRepository.FindById(entity.Id);

            // ASSERT
            Assert.Equal(entity, result);
            Assert.NotSame(entity, result);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Tags.Single().Id, result.Tags.Single().Id);
            Assert.Equal(entity.Tags.Single().Name, result.Tags.Single().Name);
            Assert.Equal(entity.Tags.Single().Facet.Name, result.Tags.Single().Facet.Name);
            Assert.Equal(entity.Tags.Single().Facet.Properties.Single().Name, result.Tags.Single().Facet.Properties.Single().Name);
            Assert.Equal(entity.Values[entity.Tags.Single().Facet.Properties.Single().Id.ToString()], result.Values[result.Tags.Single().Facet.Properties.Single().Id.ToString()]);
        }

        #endregion Entity -0:*-> PropertyValues

        #region Entity -0:1-> Category

        [Fact]
        public void EntityRespository_writes_entity_with_category()
        {
            // ARRANGE

            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var entity = DefaultEntity(WithEntityCategory(category));

            // ACT

            this.EntityRepository.Upsert(entity);

            // ASSERT

            var readEntity = this.entitiesCollection.FindById(entity.Id);

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Category.Id, readEntity["Category"].AsDocument["$id"].AsGuid);
            Assert.Equal(CategoryLiteDbRepository.collectionName, readEntity["Category"].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void EntityRespository_reads_entity_with_category_by_id()
        {
            // ARRANGE

            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(category)));

            // ACT

            var result = this.EntityRepository.FindById(entity.Id);

            // ASSERT

            Assert.Equal(category.Id, result.Category.Id);
            Assert.Equal(category, result.Category);
            Assert.NotSame(entity, result);
            Assert.NotSame(entity.Category, result.Category);
        }

        [Fact]
        public void EntityRepository_finds_entity_by_category()
        {
            // ARRANGE

            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(category)));

            // ACT

            var result = this.EntityRepository.FindByCategory(category);

            // ASSERT

            Assert.Equal(entity.Id, result.Single().Id);
        }

        [Fact]
        public void EntityRepository_finds_entity_by_category_and_name()
        {
            // ARRANGE

            var entity = this.EntityRepository.Upsert(DefaultEntity(e => e.Category = this.CategoryRepository.Root()));

            // ACT

            var result = this.EntityRepository.FindByCategoryAndName(this.CategoryRepository.Root(), entity.Name);

            // ASSERT

            Assert.Equal(entity.Id, result.Id);
        }

        #endregion Entity -0:1-> Category
    }
}