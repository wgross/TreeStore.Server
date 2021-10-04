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

        
    }
}