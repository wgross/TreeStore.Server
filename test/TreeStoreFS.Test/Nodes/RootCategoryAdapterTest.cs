using Microsoft.Extensions.DependencyInjection;
using Moq;
using PowerShellFilesystemProviderBase;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Linq;
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
            Assert.NotNull(result.Property<FacetResult>("Facet"));
            Assert.Equal(root.Facet.Properties.Single().Id, result.Property<FacetResult>("Facet").Properties.Single().Id);
            Assert.Equal(root.Facet.Properties.Single().Type, result.Property<FacetResult>("Facet").Properties.Single().Type);
        }

        [Fact]
        public void CategoryNodeAdapter_creates_child_category()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();

            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultCategoryModel(root, WithDefaultProperty);

            CreateCategoryRequest request = default;
            this.treeStoreServiceMock
                .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Callback<CreateCategoryRequest, CancellationToken>((r, ct) => request = r)
                .ReturnsAsync(child.ToCategoryResult());

            // ACT
            var result = this.rootCategoryAdapter.GetService<INewChildItem>().NewChildItem(child.Name, "category", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(child.Name, request.Name);
            Assert.Equal(root.Id, request.ParentId);
        }

        [Fact]
        public void CategoryNodeAdapter_creates_grandchild_category()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();
            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultCategoryModel(root, WithDefaultProperty);
            this.treeStoreServiceMock
                .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(child.ToCategoryResult());
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(child.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(child.ToCategoryResult());
            var childCategory = (ContainerNode)this.rootCategoryAdapter.GetService<INewChildItem>().NewChildItem(child.Name, "category", null);

            var grandchild = DefaultCategoryModel(child, WithDefaultProperty);

            CreateCategoryRequest request = default;
            this.treeStoreServiceMock
                .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Callback<CreateCategoryRequest, CancellationToken>((r, ct) => request = r)
                .ReturnsAsync(child.ToCategoryResult());

            // ACT
            var result = childCategory.NewChildItem(grandchild.Name, "category", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(grandchild.Name, request.Name);
            Assert.Equal(child.Id, request.ParentId);
        }

        [Fact]
        public void CategoryNodeAdapter_copies_child_category()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();
            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultCategoryModel(root);
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(child.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(child.ToCategoryResult());
            var childNode = new ContainerNode(child.Name, new CategoryNodeAdapter(this.treeStoreServiceMock.Object, child.Id));

            var copied = DefaultCategoryModel(root);

            this.treeStoreServiceMock
                .Setup(s => s.CopyCategoryToAsync(child.Id, root.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(copied.ToCategoryResult());

            // ACT
            var result = (ContainerNode)this.rootCategoryAdapter.GetService<ICopyChildItem>().CopyChildItem(childNode, new[] { "child2" });

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void CategoryNodeAdapter_copies_child_category_recursive()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithDefaultProperty);
            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultCategoryModel(root, WithDefaultProperty);
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(child.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(child.ToCategoryResult());
            var childNode = new ContainerNode(child.Name, new CategoryNodeAdapter(this.treeStoreServiceMock.Object, child.Id));

            var copied = DefaultCategoryModel(root);

            this.treeStoreServiceMock
                .Setup(s => s.CopyCategoryToAsync(child.Id, root.Id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(copied.ToCategoryResult());

            // ACT
            var result = (ContainerNode)this.rootCategoryAdapter.GetService<ICopyChildItemRecursive>().CopyChildItemRecursive(childNode, new[] { "child2" });

            // ASSERT
            Assert.NotNull(result);
        }
    }
}