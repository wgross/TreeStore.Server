using LiteDB;
using System;
using System.Linq;
using TreeStore.Model;
using Xunit;

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
            Assert.Null(resultInDb["Name"].AsString);
            Assert.False(resultInDb.ContainsKey("Parent"));
            Assert.Equal("_<root>", resultInDb["UniqueName"].AsString);
        }

        #endregion Root

        #region UPSERT

        [Fact]
        public void CategoryRepository_creates_subcategory_to_root()
        {
            // ARRANGE
            var category = new Category("category");

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
            var category = new Category("category");

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
            var category = new Category("category");

            this.CategoryRepository.Root().AddSubCategory(category);
            category.AssignFacet(new Facet("facet", new FacetProperty("prop")));

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
            Assert.True(resultInDb.ContainsKey("Facet")); // no further inspection. Feature isn't used.
        }

        [Fact]
        public void CategoryRepository_creating_fails_for_duplicate_child_name()
        {
            // ARRANGE
            var category = new Category("category");

            this.CategoryRepository.Root().AddSubCategory(category);
            category = this.CategoryRepository.Upsert(category);

            var second_category = new Category("category-2");
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
        public void CategoryRepository_reads_category_by_id_including_parent()
        {
            // ARRANGE
            var category = new Category("category");

            // just to add a parent
            this.CategoryRepository.Root().AddSubCategory(category);
            this.CategoryRepository.Upsert(category);

            // ACT
            var result = this.CategoryRepository.FindById(category.Id);

            // ASSERT
            Assert.NotSame(category, result);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(this.CategoryRepository.Root().Id, result.Parent.Id);
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
            var category = DefaultCategory();
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
            var category = DefaultCategory();
            this.CategoryRepository.Upsert(category);

            // ACT
            var result = this.CategoryRepository.FindByParentAndName(new Category(), "name");

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void CategoryRepository_reads_category_by_parent()
        {
            // ARRANGE
            var category = DefaultCategory();
            this.CategoryRepository.Upsert(category);
            var subcategory = DefaultCategory(WithParentCategory(category), c => c.Name = "sub");
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

            var category = DefaultCategory(c => c.Name = "cat");
            this.CategoryRepository.Upsert(category);
            var subcategory = DefaultCategory(WithParentCategory(category), c => c.Name = "sub");
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

            var category = DefaultCategory();
            this.CategoryRepository.Upsert(category);

            // ACT

            var result = this.CategoryRepository.FindByParent(new Category());

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
            var category = DefaultCategory();

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
            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var child_category = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(category)));

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
            var category = this.CategoryRepository.Upsert(DefaultCategory());
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
            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var child_category = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(category)));

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
            var category = this.CategoryRepository.Upsert(DefaultCategory());
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
            var src = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "dst"));
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
            var assert_dst_src_entity = this.EntityRepository.FindByCategoryAndName(assert_dst_src, src_entity.Name);

            Assert.Equal(src_entity.Name, assert_dst_src_entity.Name);
            Assert.NotEqual(src_entity.Id, assert_dst_src_entity.Id);
        }

        [Fact]
        public void CategoryRepository_copies_category_without_entity()
        {
            // ARRANGE
            var root = this.CategoryRepository.Root();
            var src = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "dst"));
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
            var root = this.CategoryRepository.Root();
            var src = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "dst"));
            var src_category = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(src)));

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
            var src = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "dst"));
            var src_category = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(src)));

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
            var src = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "src"));
            var dst = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(root), c => c.Name = "dst"));
            var dst_duplicate = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(dst), c => c.Name = src.Name));
            var src_category = this.CategoryRepository.Upsert(DefaultCategory(WithParentCategory(src)));

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