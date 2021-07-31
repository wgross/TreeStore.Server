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
        private readonly Mock<ITreeStoreModel> modelMock;
        private readonly TreeStoreService service;
        private Category rootCategory;

        public TreeStoreServiceTest()
        {
            this.categoryRepositoryMock = this.Mocks.Create<ICategoryRepository>();
            this.modelMock = this.Mocks.Create<ITreeStoreModel>();
            this.service = new TreeStoreService(this.modelMock.Object, new NullLogger<TreeStoreService>());
        }

        #region Category

        [Fact]
        public void TreeStoreService_reads_root_category()
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
        public async Task TreeStoreService_creates_category()
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
        public async Task TreeStoreService_creating_category_fails_if_parent_doesnt_exists()
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
        public async Task TreeStoreService_reads_category()
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
            Assert.Equal(category.ToCategoryResponse(), result);
        }

        [Fact]
        public async Task TreeStoreService_reading_category_returns_null_if_missing()
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
        public async Task TreeStoreService_updates_category()
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
        public async Task TreeStoreService_updating_category_fails_on_missing_category()
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
        [InlineData(false)]
        [InlineData(true)]
        public async Task TreeStoreService_deletes_category_if_empty(bool recurse)
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
                    .Returns(true);
            });

            // ACT
            var result = await this.service.DeleteCategoryAsync(category.Id, recurse, CancellationToken.None);

            // ASSERT
            Assert.True(result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TreeStoreService_deleting_category_returns_false_on_missing_category(bool recurse)
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
        public async Task TreeStoreService_copies_category(bool recurse)
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
        public async Task TreeStoreService_copying_category_fails_on_missing_source(bool recurse)
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
        public async Task TreeStoreService_copying_category_fails_on_missing_destination(bool recurse)
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
    }
}