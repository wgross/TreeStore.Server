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
    public class EntityNodeAdapterTest : NodeBaseTest
    {
        private readonly Mock<ITreeStoreService> treeStoreService;

        public EntityNodeAdapterTest()
        {
            this.treeStoreService = this.Mocks.Create<ITreeStoreService>();
        }

        [Fact]
        public void Get_item()
        {
            // ARRANGE
            var category = DefaultRootCategoryModel();
            var entity = DefaultEntityModel(WithEntityCategory(category));

            this.treeStoreService
                .Setup(s => s.GetEntityByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToEntityResult());

            var entityNodeAdapter = new EntityNodeAdapter(this.treeStoreService.Object, entity.Id);

            // ACT
            var result = entityNodeAdapter.GetRequiredService<IGetItem>().GetItem();

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Property<Guid>("Id"));
            Assert.Equal(entity.Name, result.Property<string>("Name"));
            Assert.Equal(entity.Category.Id, result.Property<Guid>("CategoryId"));
        }
    }
}