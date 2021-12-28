using Microsoft.Extensions.DependencyInjection;
using Moq;
using PowerShellFilesystemProviderBase;
using PowerShellFilesystemProviderBase.Capabilities;
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
    public class CategoryNodeAdapterFacetTest : NodeBaseTest
    {
        private readonly Mock<ITreeStoreService> treeStoreServiceMock;
        private readonly RootCategoryAdapter rootCategoryAdapter;

        public CategoryNodeAdapterFacetTest()
        {
            this.treeStoreServiceMock = this.Mocks.Create<ITreeStoreService>();
            this.rootCategoryAdapter = new RootCategoryAdapter(this.treeStoreServiceMock.Object);
        }

        #region CREATE

        [Fact]
        public void Creates_facet_property()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();

            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            UpdateCategoryRequest request = default;
            this.treeStoreServiceMock
                .Setup(s => s.UpdateCategoryAsync(root.Id, It.IsAny<UpdateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, UpdateCategoryRequest, CancellationToken>((_, r, _) => request = r)
                .ReturnsAsync(root.ToCategoryResult());

            // ACT
            this.rootCategoryAdapter.GetService<INewItemProperty>().NewItemProperty("p1", "long", null);

            // ASSERT
            Assert.Single(request.Facet.Creates);
            Assert.Equal("p1", request.Facet.Creates.Single().Name);
            Assert.Equal(FacetPropertyTypeValues.Long, request.Facet.Creates.Single().Type);
        }

        [Fact]
        public void Creates_facet_property_fails_on_duplicate_name()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithDefaultProperty);

            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            // ACT
            var result = Assert.Throws<InvalidOperationException>(
                () => this.rootCategoryAdapter.GetService<INewItemProperty>().NewItemProperty("guid", "long", null));

            // ASSERT
            Assert.Equal($"Creating property(name='guid') failed: property name is duplicate", result.Message);
        }

        [Fact]
        public void Creates_facet_property_fails_on_unknown_type()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();

            // ACT
            var result = Assert.Throws<InvalidOperationException>(
                () => this.rootCategoryAdapter.GetService<INewItemProperty>().NewItemProperty("p1", "unknown", null));

            // ASSERT
            Assert.Equal($"FacetProperty(name='p1') wasn't created: type 'unknown' is unknown", result.Message);
        }

        #endregion CREATE

        #region UPDATE

        [Fact]
        public void Rename_facet_property()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithDefaultProperty);

            this.treeStoreServiceMock
               .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(root.ToCategoryResult());

            UpdateCategoryRequest request = default;
            this.treeStoreServiceMock
                .Setup(s => s.UpdateCategoryAsync(root.Id, It.IsAny<UpdateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, UpdateCategoryRequest, CancellationToken>((_, r, _) => request = r)
                .ReturnsAsync(root.ToCategoryResult());

            // ACT
            this.rootCategoryAdapter.GetService<IRenameItemProperty>().RenameItemProperty("guid", "newname");

            // ASSERT
            Assert.Single(request.Facet.Updates);
            Assert.Equal("newname", request.Facet.Updates.Single().Name);
            Assert.Equal(root.FacetProperties().Single().Id, request.Facet.Updates.Single().Id);
        }

        [Fact]
        public void Rename_facet_property_fails_on_missing_property()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithDefaultProperty);

            this.treeStoreServiceMock
               .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(root.ToCategoryResult());

            // ACT
            var result = Assert.Throws<InvalidOperationException>(
                () => this.rootCategoryAdapter.GetService<IRenameItemProperty>().RenameItemProperty("unknown", "newname"));

            // ASSERT
            Assert.Equal($"Renaming property(name='unknown') failed: property doesn't exist", result.Message);
        }

        [Fact]
        public void Rename_facet_property_fails_on_duplicate_property()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithDefaultProperties);

            this.treeStoreServiceMock
               .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(root.ToCategoryResult());

            // ACT
            var result = Assert.Throws<InvalidOperationException>(
                () => this.rootCategoryAdapter.GetService<IRenameItemProperty>().RenameItemProperty("guid", "bool"));

            // ASSERT
            Assert.Equal($"Renaming property(name='guid') failed: property name is duplicate", result.Message);
        }

        #endregion UPDATE
    }
}