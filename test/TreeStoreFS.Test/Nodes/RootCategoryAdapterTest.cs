using Microsoft.Extensions.DependencyInjection;
using Moq;
using PowerShellFilesystemProviderBase;
using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Threading;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using TreeStoreFS.Nodes;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStoreFS.Test.Nodes
{
    public class RootCategoryAdapterTest : NodeBaseTest
    {
        private readonly Mock<ITreeStoreService> treeStoreServiceMock;
        private readonly RootCategoryAdapter rootCategoryAdapter;

        public RootCategoryAdapterTest()
        {
            this.treeStoreServiceMock = this.Mocks.Create<ITreeStoreService>();
            this.rootCategoryAdapter = new RootCategoryAdapter(this.treeStoreServiceMock.Object);
        }

        [Fact]
        public void RootCategoryNodeAdpater_reads_root_catgory()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithDefaultProperty);

            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            // ACT
            var result = this.rootCategoryAdapter.GetService<IGetItem>().GetItem();

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(root.Id, result.Property<Guid>("Id"));
            Assert.Equal(root.Name, result.Property<string>("Name"));
        }

        [Fact]
        public void RootCategoryNodeAdpater_create_child_category_by_default()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithDefaultProperty);

            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultCategoryModel(root);

            CreateCategoryRequest request = default;
            this.treeStoreServiceMock
                .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Callback<CreateCategoryRequest, CancellationToken>((r, _) => request = r)
                .ReturnsAsync(child.ToCategoryResult());

            // ACT
            var result = this.rootCategoryAdapter.GetService<INewChildItem>().NewChildItem("child", null, null);

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void RootCategoryNodeAdpater_creating_child_entity_fails()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithDefaultProperty);

            // ACT
            var result = Assert.Throws<InvalidOperationException>(() => this.rootCategoryAdapter.GetService<INewChildItem>().NewChildItem("child", "entity", null));

            // ASSERT
            Assert.Equal("Can't create entities in drive root", result.Message);
        }
    }
}