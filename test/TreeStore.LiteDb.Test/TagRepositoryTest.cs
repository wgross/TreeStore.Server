using LiteDB;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using TreeStore.Model;
using Xunit;

namespace TreeStore.LiteDb.Test
{
    public class TagRepositoryTest : LiteDbTestBase
    {
        private readonly TagLiteDbRepository repository;
        private readonly ILiteCollection<BsonDocument> tags;

        public TagRepositoryTest()
        {
            this.repository = new TagLiteDbRepository(this.Persistence.LiteRepository, new NullLogger<TagLiteDbRepository>());
            this.tags = this.Persistence.LiteRepository.Database.GetCollection(TagLiteDbRepository.CollectionName);
        }

        [Fact]
        public void TagRepository_writes_Tag_to_repository()
        {
            // ARRANGE
            var tag = DefaultTag(WithoutProperties);

            // ACT
            this.repository.Upsert(tag);

            // ASSERT
            var result = this.tags.FindById(tag.Id);

            Assert.NotNull(result);
            Assert.Equal(tag.Id, result.AsDocument["_id"].AsGuid);
            Assert.Equal(tag.Name, result.AsDocument["name"].AsString);
        }

        [Fact]
        public void TagRepository_writes_and_reads_Tag_with_Facet_from_repository()
        {
            // ARRANGE
            var tag = DefaultTag(WithDefaultProperty);

            // ACT
            this.repository.Upsert(tag);

            // ASSERT
            var result = this.tags.FindById(tag.Id);

            Assert.NotNull(result);
            Assert.Equal(tag.Id, result.AsDocument["_id"].AsGuid);
            Assert.Equal(tag.Name, result.AsDocument["name"].AsString);
            Assert.Equal(tag.Facet.Id, result.BsonValue("facet", "_id").AsGuid);
            Assert.Equal(tag.Facet.Name, result.BsonValue("facet", "name").AsString);
            Assert.Single(result.BsonValue("facet", "properties").AsArray);
            Assert.Equal(tag.Facet.Properties.Single().Id, result.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("_id").AsGuid);
            Assert.Equal(nameof(FacetPropertyTypeValues.Guid), result.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("type").AsString);
        }

        [Fact]
        public void TagRepository_updates_and_reads_Tag_with_Facet_from_repository()
        {
            // ARRANGE
            var tag = DefaultTag(WithDefaultProperty);

            this.repository.Upsert(tag);

            // ACT
            tag.Name = "name2";
            tag.AssignFacet("facet2", f => f.AddProperty(new FacetProperty("prop2", FacetPropertyTypeValues.Double)));

            this.repository.Upsert(tag);

            // ASSERT
            var result = this.tags.FindById(tag.Id);

            Assert.NotNull(result);
            Assert.Equal(tag.Id, result.AsDocument["_id"].AsGuid);
            Assert.Equal(tag.Name, result.AsDocument["name"].AsString);
            Assert.Equal(tag.Facet.Id, result.BsonValue("facet", "_id").AsGuid);
            Assert.Equal(tag.Facet.Name, result.BsonValue("facet", "name").AsString);
            Assert.Single(result.BsonValue("facet", "properties").AsArray);
            Assert.Equal(tag.Facet.Properties.Single().Id, result.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("_id").AsGuid);
            Assert.Equal(nameof(FacetPropertyTypeValues.Double), result.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("type").AsString);
        }

        [Fact]
        public void TagRepository_rejects_duplicate_name()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.repository.Upsert(tag);

            // ACT
            var result = Assert.Throws<LiteException>(() => this.repository.Upsert(new Tag("TAG")));

            // ASSERT
            Assert.Equal("Cannot insert duplicate key in unique index 'Name'. The duplicate value is '\"tag\"'.", result.Message);
            Assert.Single(this.tags.FindAll());
        }

        [Fact]
        public void TagRepository_finds_all_tags()
        {
            // ARRANGE
            var tag = DefaultTag(WithDefaultProperty);

            this.repository.Upsert(tag);

            // ACT
            var result = this.repository.FindAll();

            // ASSERT
            Assert.Equal(tag, result.Single());
            Assert.NotNull(result);
            Assert.Equal(tag.Id, result.Single().Id);
            Assert.Equal(tag.Name, result.Single().Name);
            Assert.Equal(tag.Facet.Id, result.Single().Facet.Id);
            Assert.Equal(tag.Facet.Name, result.Single().Facet.Name);
            Assert.Single(result.Single().Facet.Properties);
            Assert.Equal(tag.Facet.Properties.Single().Id, result.Single().Facet.Properties.Single().Id);
            Assert.Equal(FacetPropertyTypeValues.Guid, result.Single().Facet.Properties.Single().Type);
        }

        [Fact]
        public void TagRepository_finding_tag_by_unknown_id_returns_null()
        {
            // ACT
            var result = this.repository.FindById(Guid.NewGuid());

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void TagRepository_removes_tag_from_repository()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.repository.Upsert(tag);

            // ACT
            var result = this.repository.Delete(tag);

            // ASSERT
            Assert.True(result);
            Assert.Null(this.tags.FindById(tag.Id));
        }

        [Fact]
        public void TagRepository_removing_unknown_tag_returns_false()
        {
            // ARRANGE
            var tag = DefaultTag();

            // ACT
            var result = this.repository.Delete(tag);

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void TagRepository_finds_by_name()
        {
            // ARRANGE
            var tag = DefaultTag(WithDefaultProperty);

            this.repository.Upsert(tag);

            // ACT
            var result = this.repository.FindByName("tag");

            // ASSERT
            Assert.Equal(tag, result);
            Assert.NotNull(result);
            Assert.Equal(tag.Id, result.Id);
            Assert.Equal(tag.Name, result.Name);
            Assert.Equal(tag.Facet.Id, result.Facet.Id);
            Assert.Equal(tag.Facet.Name, result.Facet.Name);
            Assert.Single(result.Facet.Properties);
            Assert.Equal(tag.Facet.Properties.Single().Id, result.Facet.Properties.Single().Id);
            Assert.Equal(FacetPropertyTypeValues.Guid, result.Facet.Properties.Single().Type);
        }

        [Fact]
        public void TagRepository_finding_by_name_returns_null_on_missing_tag()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.repository.Upsert(tag);

            // ACT
            var result = this.repository.FindByName("unknown");

            // ASSERT
            Assert.Null(result);
        }
    }
}