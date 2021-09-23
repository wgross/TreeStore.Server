using Microsoft.Extensions.DependencyInjection;
using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using System.Linq;
using System.Threading;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using TreeStoreFS.Nodes;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStoreFS.Test.Nodes
{
    public class CategoryNodeAdapterTest : NodeBaseTest
    {
        private readonly Mock<ITreeStoreService> treeStoreServiceMock;

        public CategoryNodeAdapterTest()
        {
            this.treeStoreServiceMock = this.Mocks.Create<ITreeStoreService>();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Removes_child_category(bool recurse)
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult());
            var parentCategory = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            this.treeStoreServiceMock
                .Setup(s => s.DeleteCategoryAsync(category.Parent.Id, category.Name, recurse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            parentCategory.GetRequiredService<IRemoveChildItem>().RemoveChildItem(category.Name, recurse);
        }

        [Fact]
        public void Reads_child_categories_without_children()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult());
            var parentCategory = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            // ACT
            var result = parentCategory.GetRequiredService<IGetChildItems>().GetChildItems();

            // ASSERT
            Assert.False(parentCategory.HasChildItems());
            Assert.Empty(result);
        }

        [Fact]
        public void Reads_child_categories_with_entities()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var entity = DefaultEntityModel(WithEntityCategory(category));
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: null, entities: new[] { entity }));
            var parentCategory = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            this.treeStoreServiceMock
              .Setup(s => s.GetCategoriesByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new[] { category.ToCategoryResult() });

            // ACT
            var result = parentCategory.GetRequiredService<IGetChildItems>().GetChildItems();

            // ASSERT
            Assert.True(parentCategory.HasChildItems());
            Assert.NotNull(result);
            Assert.Equal(category.Name, result.Single().Name);
        }

        [Fact]
        public void Reads_child_categories_with_categoriee()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var subcatageory = DefaultCategoryModel(category);
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: new[] { subcatageory }, entities: null));
            var parentCategory = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            this.treeStoreServiceMock
              .Setup(s => s.GetCategoriesByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new[] { category.ToCategoryResult() });

            // ACT
            var result = parentCategory.GetRequiredService<IGetChildItems>().GetChildItems();

            // ASSERT
            Assert.True(parentCategory.HasChildItems());
        }

        [Fact]
        public void Reading_child_categories_returns_empty_list_on_unkown_parent()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var subcatageory = DefaultCategoryModel(category);

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: new[] { subcatageory }, entities: null));
            var parentNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoriesByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CategoryResult[])null);

            // ACT
            var result = parentNode.GetRequiredService<IGetChildItems>().GetChildItems();

            // ASSERT
            Assert.Empty(result);
        }
    }
}