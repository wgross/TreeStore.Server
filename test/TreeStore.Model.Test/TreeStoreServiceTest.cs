using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;
using TreeStore.Model.Test.Base;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Model.Test
{
    public class TreeStoreServiceTest : TreeStoreModelTestBase
    {
        private readonly Mock<ICategoryRepository> categoryRepositoryMock;
        private readonly Mock<IEntityRepository> entityRepositoryMock;
        private readonly Mock<ITagRepository> tagRepositoryMock;
        private readonly Mock<ITreeStoreModel> modelMock;
        private readonly TreeStoreService service;
        private Category rootCategory;

        public TreeStoreServiceTest()
        {
            this.categoryRepositoryMock = this.Mocks.Create<ICategoryRepository>();
            this.entityRepositoryMock = this.Mocks.Create<IEntityRepository>();
            this.tagRepositoryMock = this.Mocks.Create<ITagRepository>();
            this.modelMock = this.Mocks.Create<ITreeStoreModel>();
            this.service = new TreeStoreService(this.modelMock.Object, new NullLogger<TreeStoreService>());
        }

        #region Category

        [Fact]
        public void Reads_root_category()
        {
            // ARRANGE
            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.Root())
                    .Returns(DefaultRootCategory());
            });

            // ACT
            var result = this.service.GetRootCategory();

            // ASSERT
            Assert.Same(this.DefaultRootCategory(), result);
        }

        [Fact]
        public async Task Creates_category()
        {
            // ARRANGE
            var rootCategory = this.ArrangeRootCategory();
            var category = DefaultCategory(this.rootCategory);

            Category categoryWritten = default;

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.Upsert(It.IsAny<Category>()))
                    .Callback<Category>(c => categoryWritten = c)
                    .Returns<Category>(c => c);
            });

            // ACT
            var result = await this.service.CreateCategoryAsync(new CreateCategoryRequest(category.Name, this.rootCategory.Id), CancellationToken.None);

            // ASSERT
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(rootCategory.Id, result.ParentId);
            Assert.Equal(category.Name, categoryWritten.Name);
            Assert.Equal(rootCategory.Id, categoryWritten.Parent.Id);
        }

        [Fact]
        public async Task Creating_category_fails_if_parent_doesnt_exists()
        {
            // ARRANGE
            var parentId = Guid.NewGuid();
            var category = DefaultCategory(DefaultRootCategory());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(parentId))
                    .Returns((Category)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
               this.service.CreateCategoryAsync(new CreateCategoryRequest(category.Name, parentId),
               CancellationToken.None));

            // ASSERT
            Assert.Equal($"Category(name='{category.Name}' wasn't created: Category(id='{parentId}') wasn't found", result.Message);
        }

        [Fact]
        public async Task Reads_category()
        {
            // ARRANGE
            var category = DefaultCategory(DefaultRootCategory());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns(category);
            });

            // ACT
            var result = await this.service.GetCategoryByIdAsync(category.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(category.ToCategoryResult(), result);
        }

        [Fact]
        public async Task Reading_category_returns_null_if_missing()
        {
            // ARRANGE
            var category = DefaultCategory(DefaultRootCategory());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns((Category)null);
            });

            // ACT
            var result = await this.service.GetCategoryByIdAsync(category.Id, CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Updates_category()
        {
            // ARRANGE
            var category = DefaultCategory(DefaultRootCategory());
            Category writtenCategory = default;

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns(category);

                mock
                    .Setup(r => r.Upsert(It.IsAny<Category>()))
                    .Callback<Category>(c => writtenCategory = c)
                    .Returns(category);
            });

            // ACT
            var result = await this.service.UpdateCategoryAsync(category.Id, new UpdateCategoryRequest(Name: "changed"), CancellationToken.None);

            // ASSERT
            Assert.Equal("changed", writtenCategory.Name);
            Assert.Equal(category.Id, writtenCategory.Id);
            Assert.Equal(category.Parent.Id, writtenCategory.Parent.Id);
        }

        [Fact]
        public async Task Updating_category_fails_on_missing_category()
        {
            // ARRANGE
            var category = DefaultCategory(DefaultRootCategory());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns((Category)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.service.UpdateCategoryAsync(category.Id, new UpdateCategoryRequest(Name: "changed"), CancellationToken.None));

            // ASSERT
            Assert.Equal($"Category(id='{category.Id}') wasn't updated: Category(id='{category.Id}') doesn't exist", result.Message);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Deletes_category_if_empty(bool recurse, bool deleteResult)
        {
            // ARRANGE
            var category = DefaultCategory(DefaultRootCategory());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns(category);

                mock
                    .Setup(r => r.Delete(category, recurse))
                    .Returns(deleteResult);
            });

            // ACT
            var result = await this.service.DeleteCategoryAsync(category.Id, recurse, CancellationToken.None);

            // ASSERT
            Assert.Equal(deleteResult, result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Deleting_category_returns_false_on_missing_category(bool recurse)
        {
            // ARRANGE
            var category = DefaultCategory(DefaultRootCategory());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns((Category)null);
            });

            // ACT
            var result = await this.service
                .DeleteCategoryAsync(category.Id, recurse, CancellationToken.None);

            // ASSERT
            Assert.False(result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Copies_category(bool recurse)
        {
            // ARRANGE
            var source = DefaultCategory(DefaultRootCategory());
            var destination = DefaultCategory(DefaultRootCategory());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(source.Id))
                    .Returns(source);

                mock
                    .Setup(r => r.FindById(destination.Id))
                    .Returns(destination);

                mock
                    .Setup(r => r.CopyTo(source, destination, recurse));
            });

            // ACT
            var result = await this.service
                .CopyCategoryToAsync(source.Id, destination.Id, recurse, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Copying_category_fails_on_missing_source(bool recurse)
        {
            // ARRANGE
            var source = DefaultCategory(DefaultRootCategory());
            var destination = DefaultCategory(DefaultRootCategory());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(source.Id))
                    .Returns((Category)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service
                 .CopyCategoryToAsync(source.Id, destination.Id, recurse, CancellationToken.None));

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal($"Category(id='{source.Id}') wasn't copied: Category(id='{source.Id}') doesn't exist", result.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Copying_category_fails_on_missing_destination(bool recurse)
        {
            // ARRANGE
            var source = DefaultCategory(DefaultRootCategory());
            var destination = DefaultCategory(DefaultRootCategory());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(source.Id))
                    .Returns(source);

                mock
                    .Setup(r => r.FindById(destination.Id))
                    .Returns((Category)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service
                 .CopyCategoryToAsync(source.Id, destination.Id, recurse, CancellationToken.None));

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal($"Category(id='{source.Id}') wasn't copied: Category(id='{destination.Id}') doesn't exist", result.Message);
        }

        private void ArrangeCategoryRepository(Action<Mock<ICategoryRepository>> arrange = null)
        {
            this.modelMock
                .Setup(m => m.Categories)
                .Returns(this.categoryRepositoryMock.Object);

            arrange?.Invoke(this.categoryRepositoryMock);
        }

        private Category ArrangeRootCategory()
        {
            var root = DefaultRootCategory();
            this.categoryRepositoryMock
                .Setup(r => r.FindById(root.Id))
                .Returns(root);

            return root;
        }

        private Category DefaultRootCategory()
        {
            this.rootCategory ??= new Category();
            return this.rootCategory;
        }

        #endregion Category

        #region Entity

        [Fact]
        public async Task Reads_entity()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultRootCategory());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);
            });

            // ACT
            var result = await this.service.GetEntityByIdAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.ToEntityResult(), result);
        }

        [Fact]
        public async Task Reading_entity_returns_null_if_missing()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultRootCategory());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns((Entity)null);
            });

            // ACT
            var result = await this.service.GetEntityByIdAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Updates_entity()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultRootCategory());

            Entity writtenEntity = default;

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);

                mock
                    .Setup(r => r.Upsert(It.IsAny<Entity>()))
                    .Callback<Entity>(c => writtenEntity = c)
                    .Returns(entity);
            });

            // ACT
            var result = await this.service.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(Name: "changed"), CancellationToken.None);

            // ASSERT
            Assert.Equal("changed", writtenEntity.Name);
            Assert.Equal(entity.Id, writtenEntity.Id);
            Assert.Equal(entity.Category.Id, writtenEntity.Category.Id);
        }

        [Fact]
        public async Task Updating_entity_fails_on_missing_entity()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultRootCategory());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns((Entity)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(Name: "changed"), CancellationToken.None));

            // ASSERT
            Assert.Equal($"Entity(id='{entity.Id}') wasn't updated: Entity(id='{entity.Id}') doesn't exist", result.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Deletes_entity(bool deleteResult)
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultRootCategory());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);

                mock
                    .Setup(r => r.Delete(entity))
                    .Returns(deleteResult);
            });

            // ACT
            var result = await this.service.DeleteEntityAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(deleteResult, result);
        }

        [Fact]
        public async Task Deleting_entity_returns_false_if_entity_missing()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultRootCategory());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns((Entity)null);
            });

            // ACT
            var result = await this.service.DeleteEntityAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.False(result);
        }

        private void ArrangeEntityRepository(Action<Mock<IEntityRepository>> arrange)
        {
            this.modelMock
                .Setup(m => m.Entities)
                .Returns(this.entityRepositoryMock.Object);

            arrange?.Invoke(this.entityRepositoryMock);
        }

        #endregion Entity

        #region Tag

        [Fact]
        public async Task Reads_tag()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns(tag);
            });

            // ACT
            var result = await this.service.GetTagByIdAsync(tag.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(tag.ToTagResult(), result);
        }

        [Fact]
        public async Task Reading_tag_returns_null_if_missing()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns((Tag)null);
            });

            // ACT
            var result = await this.service.GetTagByIdAsync(tag.Id, CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Updates_tag()
        {
            // ARRANGE
            var tag = DefaultTag();

            Tag writtenTag = default;

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns(tag);

                mock
                    .Setup(r => r.Upsert(It.IsAny<Tag>()))
                    .Callback<Tag>(c => writtenTag = c)
                    .Returns(tag);
            });

            // ACT
            var result = await this.service.UpdateTagAsync(tag.Id, new UpdateTagRequest(Name: "changed"), CancellationToken.None);

            // ASSERT
            Assert.Equal("changed", writtenTag.Name);
            Assert.Equal(tag.Id, writtenTag.Id);
        }

        [Fact]
        public async Task Updating_tag_fails_on_missing_tag()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns((Tag)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service.UpdateTagAsync(tag.Id, new UpdateTagRequest(Name: "changed"), CancellationToken.None));

            // ASSERT
            Assert.Equal($"Tag(id='{tag.Id}') wasn't updated: Tag(id='{tag.Id}') doesn't exist", result.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Deletes_tag(bool deleteResult)
        {
            // ARRANGE
            var tag = DefaultTag();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns(tag);

                mock
                    .Setup(r => r.Delete(tag))
                    .Returns(deleteResult);
            });

            // ACT
            var result = await this.service.DeleteTagAsync(tag.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(deleteResult, result);
        }

        [Fact]
        public async Task Deleting_tag_returns_false_if_tag_missing()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns((Tag)null);
            });

            // ACT
            var result = await this.service.DeleteTagAsync(tag.Id, CancellationToken.None);

            // ASSERT
            Assert.False(result);
        }

        private void ArrangeTagRepository(Action<Mock<ITagRepository>> arrange)
        {
            this.modelMock
                .Setup(m => m.Tags)
                .Returns(this.tagRepositoryMock.Object);

            arrange?.Invoke(this.tagRepositoryMock);
        }

        #endregion Tag
    }
}