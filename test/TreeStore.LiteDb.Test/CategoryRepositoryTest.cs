using LiteDB;
using System;
using System.Linq;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.LiteDb.Test
{
    public class CategoryRepositoryTest : LiteDbTestBase
    {
        private readonly ILiteCollection<BsonDocument> categoriesCollection;

        public CategoryRepositoryTest()
        {
            this.categoriesCollection = this.Persistence.LiteRepository.Database.GetCollection("categories");
        }

        #region Root

        [Fact]
        public void CategoryRepository_provides_persistent_root()
        {
            // ACT
            var result = this.CategoryRepository.Root();

            // ASSERT
            Assert.NotNull(result);
            Assert.Empty(result.Name);

            // root was created
            var resultInDb = this.categoriesCollection.FindById(result.Id);

            Assert.NotNull(resultInDb);

            // the category document has expected content
            Assert.Equal(result.Id, resultInDb["_id"].AsGuid);
            Assert.Equal("", resultInDb["Name"].AsString);
            Assert.False(resultInDb.ContainsKey("Parent"));
            Assert.Equal("_<root>", resultInDb["UniqueName"].AsString);
            Assert.Equal(result.Facet.Id, resultInDb.BsonValue("Facet", "_id").AsGuid);
        }

        [Fact]
        public void CategoryRepository_uses_persistent_root()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();

            // ACT
            var result = this.CategoryRepository.Root();

            // ASSERT
            Assert.Equal(root, result);
            Assert.NotSame(root, result);
            Assert.Equal(root.Id, result.Id);
        }

        [Fact]
        public void CatageoryRepostory_updates_root_facet()
        {
            var root = this.CategoryRepository.Root();
            WithDefaultProperty(root);

            // ACT
            var result = this.CategoryRepository.Upsert(root);

            // ASSERT
            Assert.NotNull(result);
            Assert.Empty(result.Name);

            // root was created
            var resultInDb = this.categoriesCollection.FindById(result.Id);

            Assert.NotNull(resultInDb);

            // the category document has expected content
            Assert.Equal(result.Id, resultInDb["_id"].AsGuid);
            Assert.Equal("", resultInDb["Name"].AsString);
            Assert.False(resultInDb.ContainsKey("Parent"));
            Assert.Equal("_<root>", resultInDb["UniqueName"].AsString);
            Assert.True(resultInDb.ContainsKey("Facet"));
            Assert.Equal(root.Facet.Id, resultInDb.BsonValue("facet", "_id").AsGuid);
            Assert.Equal(root.Facet.Name, resultInDb.BsonValue("facet", "name").AsString);
            Assert.Single(resultInDb.BsonValue("facet", "properties").AsArray);
            Assert.Equal(root.Facet.Properties.Single().Id, resultInDb.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("_id").AsGuid);
            Assert.Equal(nameof(FacetPropertyTypeValues.Guid), resultInDb.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("type").AsString);

            var rootFromRead = this.CategoryRepository.Root();

            Assert.Equal(root.FacetProperties().Single(), rootFromRead.Facet.Properties.Single());
        }

        #endregion Root

        #region UPSERT

        [Fact]
        public void CategoryRepository_creates_subcategory_to_root()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.CategoryRepository.Root(), WithoutProperties);

            // just to add a parent
            this.CategoryRepository.Root().AddSubCategory(category);

            // ACT
            var result = this.CategoryRepository.Upsert(category);

            // ASSERT
            Assert.Same(category, result);
            Assert.NotEqual(Guid.Empty, result.Id);

            // category was created
            var resultInDb = this.categoriesCollection.FindById(category.Id);

            Assert.NotNull(resultInDb);

            // the category document has expected content
            Assert.Equal(category.Id, resultInDb["_id"].AsGuid);
            Assert.Equal(category.Name, resultInDb["Name"].AsString);
            Assert.Equal(this.CategoryRepository.Root().Id, resultInDb["Parent"].AsDocument["$id"].AsGuid);
            Assert.Equal(category.UniqueName, resultInDb["UniqueName"].AsString);
            Assert.Equal("categories", resultInDb["Parent"].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void CategoryRepository_creating_fails_for_orphaned_category()
        {
            // ARRANGE
            var category = new CategoryModel("category");

            // ACT
            var result = Assert.Throws<InvalidOperationException>(() => this.CategoryRepository.Upsert(category));

            // ASSERT
            Assert.Equal("Category must have parent.", result.Message);
            Assert.Null(this.categoriesCollection.FindById(category.Id));
        }

        [Fact]
        public void CategoryRepository_writes_category_with_Facet()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "category", WithDefaultProperty);

            // ACT
            var result = this.CategoryRepository.Upsert(category);

            // ASSERT
            Assert.Same(category, result);
            Assert.NotEqual(Guid.Empty, result.Id);

            // category was created
            var resultInDb = this.categoriesCollection.FindById(category.Id);

            Assert.NotNull(resultInDb);

            // the category document has expected content
            Assert.Equal(category.Id, resultInDb["_id"].AsGuid);
            Assert.Equal(category.Name, resultInDb["Name"].AsString);
            Assert.Equal(this.CategoryRepository.Root().Id, resultInDb["Parent"].AsDocument["$id"].AsGuid);
            Assert.Equal("categories", resultInDb["Parent"].AsDocument["$ref"].AsString);
            Assert.Equal(category.UniqueName, resultInDb["UniqueName"].AsString);
            Assert.True(resultInDb.ContainsKey("Facet"));
            Assert.Equal(category.Facet.Id, resultInDb.BsonValue("facet", "_id").AsGuid);
            Assert.Equal(category.Facet.Name, resultInDb.BsonValue("facet", "name").AsString);
            Assert.Single(resultInDb.BsonValue("facet", "properties").AsArray);
            Assert.Equal(category.Facet.Properties.Single().Id, resultInDb.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("_id").AsGuid);
            Assert.Equal(nameof(FacetPropertyTypeValues.Guid), resultInDb.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("type").AsString);
        }

        [Fact]
        public void CategoryRepository_writes_root_category_with_Facet()
        {
            // ARRANGE
            var category = this.CategoryRepository.Root();

            WithDefaultProperty(category);

            // ACT
            var result = this.CategoryRepository.Upsert(category);

            // ASSERT
            Assert.Same(category, result);
            Assert.NotEqual(Guid.Empty, result.Id);

            // category was created
            var resultInDb = this.categoriesCollection.FindById(category.Id);

            Assert.NotNull(resultInDb);

            // the category document has expected content
            Assert.Equal(category.Id, resultInDb["_id"].AsGuid);
            Assert.Equal("", resultInDb["Name"].AsString);
            Assert.True(resultInDb["Parent"].IsNull);
            Assert.Equal(category.UniqueName, resultInDb["UniqueName"].AsString);
            Assert.True(resultInDb.ContainsKey("Facet"));
            Assert.Equal(category.Facet.Id, resultInDb.BsonValue("facet", "_id").AsGuid);
            Assert.Equal("", resultInDb.BsonValue("facet", "name"));
            Assert.Single(resultInDb.BsonValue("facet", "properties").AsArray);
            Assert.Equal(category.Facet.Properties.Single().Id, resultInDb.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("_id").AsGuid);
            Assert.Equal(nameof(FacetPropertyTypeValues.Guid), resultInDb.BsonValue("facet", "properties").AsArray.Single().AsDocument.BsonValue("type").AsString);
        }

        [Fact]
        public void CategoryRepository_creating_fails_for_duplicate_child_name()
        {
            // ARRANGE
            var category = new CategoryModel("category");

            this.CategoryRepository.Root().AddSubCategory(category);
            category = this.CategoryRepository.Upsert(category);

            var second_category = new CategoryModel("category-2");
            this.CategoryRepository.Root().AddSubCategory(second_category);
            second_category = this.CategoryRepository.Upsert(second_category);

            // ACT
            second_category.Name = category.Name;
            var result = Assert.Throws<LiteException>(() => this.CategoryRepository.Upsert(second_category));

            // ASSERT
            Assert.StartsWith(
                $"Cannot insert duplicate key in unique index 'UniqueName'. The duplicate value is '\"category_",
                result.Message);
        }

        #endregion UPSERT

        #region READ

        [Fact]
        public void CategoryRepository_reads_root_by_id()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();
            WithDefaultProperty(root);
            this.CategoryRepository.Upsert(root);

            // ACT
            var result = this.CategoryRepository.FindById(root.Id);

            // ASSERT
            Assert.NotSame(root, result);
            Assert.Equal(root.Id, result.Id);
            Assert.Null(result.Parent);
            Assert.Equal(root.Name, result.Name);
            Assert.Equal(root.Facet.Id, result.Facet.Id);
            Assert.Equal(root.Facet.Name, result.Facet.Name);
            Assert.Equal(root.Facet.Properties.Single().Id, result.Facet.Properties.Single().Id);
            Assert.Equal(root.Facet.Properties.Single().Name, result.Facet.Properties.Single().Name);
            Assert.Equal(root.Facet.Properties.Single().Type, result.Facet.Properties.Single().Type);
        }

        [Fact]
        public void CategoryRepository_reads_category_by_id_including_parent()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();
            WithDefaultProperty(root);
            this.CategoryRepository.Upsert(root);

            var category = DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "category", WithDefaultProperty);

            this.CategoryRepository.Upsert(category);

            // ACT
            var result = this.CategoryRepository.FindById(category.Id);

            // ASSERT
            Assert.NotSame(category, result);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(this.CategoryRepository.Root().Id, result.Parent.Id);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Facet.Name, result.Facet.Name);
            Assert.Equal(category.Facet.Properties.Single().Id, result.Facet.Properties.Single().Id);
            Assert.Equal(category.Facet.Properties.Single().Name, result.Facet.Properties.Single().Name);
            Assert.Equal(category.Facet.Properties.Single().Type, result.Facet.Properties.Single().Type);

            Assert.Equal(root.Id, result.Parent.Id);
            Assert.Equal(root.Name, result.Parent.Name);
            Assert.Equal(root.Facet.Id, result.Parent.Facet.Id);
            Assert.Equal(root.Facet.Name, result.Parent.Facet.Name);
        }

        [Fact]
        public void CategoryRepository_reads_category_by_id_including_2_ancestors()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();
            WithDefaultProperty(root);
            root = this.CategoryRepository.Upsert(root);

            var category1 = this.CategoryRepository.Upsert(DefaultCategoryModel(root, c => c.Name = "1", WithDefaultProperty));
            var category2 = this.CategoryRepository.Upsert(DefaultCategoryModel(category1, c => c.Name = "2", WithDefaultProperty));

            // ACT
            var result = this.CategoryRepository.FindById(category2.Id);

            // ASSERT
            Assert.Equal(category2.Id, result.Id);
            Assert.Equal(category2.Facet.Id, result.Facet.Id);
            Assert.Contains(category2.Facet.Properties.Single(), result.FacetProperties());

            Assert.Equal(category1.Id, result.Parent.Id);
            Assert.Equal(category1.Facet.Id, result.Parent.Facet.Id);
            Assert.Contains(category1.Facet.Properties.Single(), result.FacetProperties());

            Assert.Equal(root.Id, result.Parent.Parent.Id);
            Assert.Equal(root.Facet.Id, result.Parent.Parent.Facet.Id);
            Assert.Contains(root.Facet.Properties.Single(), result.FacetProperties());
        }

        [Fact]
        public void CategoryRepository_reads_category_by_id_including_3_ancestors()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();
            WithDefaultProperty(root);
            root = this.CategoryRepository.Upsert(root);

            var category1 = this.CategoryRepository.Upsert(DefaultCategoryModel(root, c => c.Name = "1", WithDefaultProperty));
            var category2 = this.CategoryRepository.Upsert(DefaultCategoryModel(category1, c => c.Name = "2", WithDefaultProperty));
            var category3 = this.CategoryRepository.Upsert(DefaultCategoryModel(category2, c => c.Name = "3", WithDefaultProperty));

            // ACT
            var result = this.CategoryRepository.FindById(category3.Id);

            // ASSERT
            Assert.Equal(category3.Id, result.Id);
            Assert.Equal(category3.Name, result.Name);
            Assert.Equal(category3.Facet.Id, result.Facet.Id);
            Assert.Contains(category3.Facet.Properties.Single(), result.FacetProperties());

            Assert.Equal(category2.Id, result.Parent.Id);
            Assert.Equal(category2.Name, result.Parent.Name);
            Assert.Equal(category2.Facet.Id, result.Parent.Facet.Id);
            Assert.Contains(category2.Facet.Properties.Single(), result.FacetProperties());

            Assert.Equal(category1.Id, result.Parent.Parent.Id);
            Assert.Equal(category1.Name, result.Parent.Parent.Name);
            Assert.Equal(category1.Facet.Id, result.Parent.Parent.Facet.Id);
            Assert.Contains(category1.Facet.Properties.Single(), result.FacetProperties());

            Assert.Equal(root.Id, result.Parent.Parent.Parent.Id);
            Assert.Equal(root.Name, result.Parent.Parent.Parent.Name);
            Assert.Equal(root.Facet.Id, result.Parent.Parent.Parent.Facet.Id);
            Assert.Contains(root.Facet.Properties.Single(), result.FacetProperties());
        }

        [Fact]
        public void CategoryRepository_reading_category_by_id_returns_null_on_missing_category()
        {
            // ACT
            var result = this.CategoryRepository.FindById(Guid.NewGuid());

            // ASSERT
            Assert.Null(result);
        }

        [Theory]
        [InlineData("c")]
        [InlineData("C")]
        public void CategoryRepository_reads_category_by_parent_and_name(string name)
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.CategoryRepository.Root());
            this.CategoryRepository.Upsert(category);

            // ACT
            var result = this.CategoryRepository.FindByParentAndName(this.CategoryRepository.Root(), name);

            // ASSERT
            Assert.Equal(category.Id, result.Id);
        }

        [Fact]
        public void CategoryRepository_reading_category_by_parent_and_name_returns_null_on_unkown_id()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.CategoryRepository.Root());
            this.CategoryRepository.Upsert(category);

            // ACT
            var result = this.CategoryRepository.FindByParentAndName(new CategoryModel(), "name");

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void CategoryRepository_reads_category_by_parent()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.CategoryRepository.Root());
            this.CategoryRepository.Upsert(category);
            var subcategory = DefaultCategoryModel(category, c => c.Name = "sub");
            category.AddSubCategory(subcategory);
            this.CategoryRepository.Upsert(subcategory);

            // ACT
            var result = this.CategoryRepository.FindByParent(this.CategoryRepository.Root());

            // ASSERT
            Assert.Equal(category.Name, result.Single().Name);
            Assert.Equal(category.Id, result.Single().Id);
        }

        [Fact]
        public void CategoryRepository_reads_subcategory_by_parent()
        {
            // ARRANGE

            var category = DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "cat");
            this.CategoryRepository.Upsert(category);
            var subcategory = DefaultCategoryModel(category, c => c.Name = "sub");
            category.AddSubCategory(subcategory);
            this.CategoryRepository.Upsert(subcategory);

            // ACT

            var result = this.CategoryRepository.FindByParent(category);

            // ASSERT

            Assert.Equal(subcategory.Id, result.Single().Id);
        }

        [Fact]
        public void CategoryRepository_reading_child_categories_by_id_is_empty_on_unknown_id()
        {
            // ARRANGE

            var category = DefaultCategoryModel(this.CategoryRepository.Root());
            this.CategoryRepository.Upsert(category);

            // ACT

            var result = this.CategoryRepository.FindByParent(new CategoryModel());

            // ASSERT

            Assert.Empty(result);
        }

        #endregion READ

        #region DELETE

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CategoryRepository_deletes_empty_category(bool recurse)
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.CategoryRepository.Root());

            // just to add a parent
            this.CategoryRepository.Root().AddSubCategory(category);
            this.CategoryRepository.Upsert(category);

            // ACT
            var result = this.CategoryRepository.Delete(category, recurse);

            // ASSERT
            Assert.True(result);
            Assert.Null(this.CategoryRepository.FindById(category.Id));
            Assert.Empty(this.CategoryRepository.FindByParent(this.CategoryRepository.Root()));
        }

        [Fact]
        public void CategoryRepository_deleting_category_fails_bc_subcategory()
        {
            // ARRANGE
            var category = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root()));
            var child_category = this.CategoryRepository.Upsert(DefaultCategoryModel(category));

            // ACT
            var result = this.CategoryRepository.Delete(category, recurse: false);

            // ASSERT
            Assert.False(result);
            Assert.NotNull(this.CategoryRepository.FindById(category.Id));
            Assert.NotNull(this.CategoryRepository.FindById(child_category.Id));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CategoryRepository_deleting_category_fails_for_root(bool recurse)
        {
            // ACT
            var result = this.CategoryRepository.Delete(this.CategoryRepository.Root(), recurse);

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void CategoryRepository_deleting_category_fails_bc_entity()
        {
            // ARRANGE
            var category = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root()));
            var entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(category)));

            // ACT
            var result = this.CategoryRepository.Delete(category, recurse: false);

            // ASSERT
            Assert.False(result);
            Assert.NotNull(this.CategoryRepository.FindById(category.Id));
            Assert.NotNull(this.EntityRepository.FindById(entity.Id));
        }

        [Fact]
        public void CategoryRepository_deleting_category_includes_subcategory()
        {
            // ARRANGE
            var category = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root()));
            var child_category = this.CategoryRepository.Upsert(DefaultCategoryModel(category));

            // ACT
            var result = this.CategoryRepository.Delete(category, recurse: true);

            // ASSERT
            Assert.True(result);
            Assert.Null(this.CategoryRepository.FindById(category.Id));
            Assert.Null(this.CategoryRepository.FindById(child_category.Id));
        }

        [Fact]
        public void CategoryRepository_deleting_category_includes_entities()
        {
            // ARRANGE
            var category = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root()));
            var entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(category)));

            // ACT
            var result = this.CategoryRepository.Delete(category, recurse: true);

            // ASSERT
            Assert.True(result);
            Assert.Null(this.CategoryRepository.FindById(category.Id));
            Assert.Null(this.EntityRepository.FindById(entity.Id));
        }

        #endregion DELETE

        #region COPY

        [Fact]
        public void CategoryRepository_copies_category_with_entity()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();
            var src = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "dst"));
            var src_entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(src)));

            // ACT
            this.CategoryRepository.CopyTo(src, dst, recurse: true);

            // ASSERT
            var assert_src = this.CategoryRepository.FindById(src.Id);

            // the category was copied
            var assert_dst_src = this.CategoryRepository.FindByParentAndName(dst, src.Name);

            Assert.NotEqual(src.Id, assert_dst_src.Id);
            Assert.Equal(src.Name, assert_dst_src.Name);

            // the entity was copied
            var assert_dst_src_entity = this.EntityRepository.FindByCategory(assert_dst_src).Single();
            //var assert_dst_src_entity = this.EntityRepository.FindByCategoryAndName(assert_dst_src, src_entity.Name);

            Assert.Equal(src_entity.Name, assert_dst_src_entity.Name);
            Assert.NotEqual(src_entity.Id, assert_dst_src_entity.Id);
        }

        [Fact]
        public void CategoryRepository_copies_category_with_facet()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();
            var src = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "src", WithDefaultProperty));
            var dst = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "dst"));

            // ACT
            this.CategoryRepository.CopyTo(src, dst, recurse: true);

            // ASSERT
            var assert_src = this.CategoryRepository.FindById(src.Id);

            // the category was copied
            var assert_dst_src = this.CategoryRepository.FindByParentAndName(dst, src.Name);

            Assert.NotEqual(src.Id, assert_dst_src.Id);
            Assert.Equal(src.Name, assert_dst_src.Name);
            Assert.Equal(src.Facet.Name, assert_dst_src.Facet.Name);
            Assert.NotEqual(src.Facet.Id, assert_dst_src.Facet.Id);

            // TODO: Wrong: propperty ids have to be changed.
            Assert.Equal(src.Facet.Properties.Single().Id, assert_dst_src.Facet.Properties.Single().Id);
            Assert.Equal(src.Facet.Properties.Single().Name, assert_dst_src.Facet.Properties.Single().Name);
            Assert.Equal(src.Facet.Properties.Single().Type, assert_dst_src.Facet.Properties.Single().Type);
        }

        [Fact]
        public void CategoryRepository_copies_category_without_entity()
        {
            // ARRANGE
            var src = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "dst"));
            var src_entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(src)));

            // ACT
            this.CategoryRepository.CopyTo(src, dst, recurse: false);

            // ASSERT
            var assert_src = this.CategoryRepository.FindById(src.Id);

            // category was copied
            var assert_dst_src = this.CategoryRepository.FindByParentAndName(dst, src.Name);

            Assert.NotEqual(src.Id, assert_dst_src.Id);
            Assert.Equal(src.Name, assert_dst_src.Name);

            // entity wasn't copied
            var assert_dst_src_entity = this.EntityRepository.FindByCategoryAndName(assert_dst_src, src_entity.Name);

            Assert.Null(assert_dst_src_entity);
        }

        [Fact]
        public void CategoryRepository_copies_category_with_subcategory()
        {
            // ARRANGE

            var src = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategoryModel(this.CategoryRepository.Root(), c => c.Name = "dst"));
            var src_category = this.CategoryRepository.Upsert(DefaultCategoryModel(src));

            // ACT
            this.CategoryRepository.CopyTo(src, dst, recurse: true);

            // ASSERT
            var assert_src = this.CategoryRepository.FindById(src.Id);

            // the category was copied
            var assert_dst_src = this.CategoryRepository.FindByParentAndName(dst, src.Name);

            Assert.NotEqual(src.Id, assert_dst_src.Id);
            Assert.Equal(src.Name, assert_dst_src.Name);

            // the category was copied
            var assert_dst_src_category = this.CategoryRepository.FindByParentAndName(assert_dst_src, src_category.Name);

            Assert.Equal(src_category.Name, assert_dst_src_category.Name);
            Assert.NotEqual(src_category.Id, assert_dst_src_category.Id);
        }

        [Fact]
        public void CategoryRepository_copies_category_without_subcategory()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();
            var src = this.CategoryRepository.Upsert(DefaultCategoryModel(root, c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategoryModel(root, c => c.Name = "dst"));
            var src_category = this.CategoryRepository.Upsert(DefaultCategoryModel(src));

            // ACT
            this.CategoryRepository.CopyTo(src, dst, recurse: false);

            // ASSERT
            var assert_src = this.CategoryRepository.FindById(src.Id);

            // the category was copied
            var assert_dst_src = this.CategoryRepository.FindByParentAndName(dst, src.Name);

            Assert.NotEqual(src.Id, assert_dst_src.Id);
            Assert.Equal(src.Name, assert_dst_src.Name);

            // the category was copied
            var assert_dst_src_category = this.CategoryRepository.FindByParentAndName(assert_dst_src, src_category.Name);

            Assert.Null(assert_dst_src_category);
        }

        [Fact]
        public void CategoryRepository_copying_fails_for_duplicate_category_name()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();
            var src = this.CategoryRepository.Upsert(DefaultCategoryModel(root, c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategoryModel(root, c => c.Name = "dst"));
            var dst_duplicate = this.CategoryRepository.Upsert(DefaultCategoryModel(dst, c => c.Name = src.Name));
            var src_category = this.CategoryRepository.Upsert(DefaultCategoryModel(src));

            // ACT
            var result = Assert.Throws<LiteException>(() => this.CategoryRepository.CopyTo(src, dst, recurse: true));

            // ASSERT
            Assert.StartsWith(
                expectedStartString: $"Cannot insert duplicate key in unique index 'UniqueName'.",
                actualString: result.Message);
        }

        #endregion COPY
    }
}
;