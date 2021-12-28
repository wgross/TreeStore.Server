using Microsoft.Extensions.DependencyInjection;
using Moq;
using PowerShellFilesystemProviderBase;
using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Linq;
using System.Management.Automation;
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

        #region IGetItem

        [Fact]
        public void Get_item()
        {
            // ARRANGE
            var category = DefaultRootCategoryModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithEntityCategory(category));

            var value = Guid.NewGuid();
            entity.SetFacetProperty(category.FacetProperties().Single(), value);

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
            Assert.Equal(value, result.Property<Guid?>("guid"));
        }

        #endregion IGetItem

        #region ISetItemProperty

        [Fact]
        public void Set_item_property_value()
        {
            // ARRANGE
            var category = DefaultRootCategoryModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithEntityCategory(category));

            this.treeStoreService
                .Setup(s => s.GetEntityByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToEntityResult());

            UpdateEntityRequest result = null;
            this.treeStoreService
                .Setup(s => s.UpdateEntityAsync(entity.Id, It.IsAny<UpdateEntityRequest>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, UpdateEntityRequest, CancellationToken>((_, updt, _) => result = updt)
                .ReturnsAsync(entity.ToEntityResult());

            var entityNodeAdapter = new EntityNodeAdapter(this.treeStoreService.Object, entity.Id);

            var value = Guid.NewGuid();
            entity.SetFacetProperty(category.FacetProperties().Single(), value);

            // ACT
            entityNodeAdapter.GetRequiredService<ISetItemProperty>().SetItemProperty(PSObject.AsPSObject(new { guid = value }));

            // ASSERT
            Assert.Equal(value, result.Values.Updates.Single().Value);
        }

        #endregion ISetItemProperty

        #region IClearItemProperty

        [Fact]
        public void Clear_item_property_value()
        {
            // ARRANGE
            var category = DefaultRootCategoryModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithEntityCategory(category));

            this.treeStoreService
                .Setup(s => s.GetEntityByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToEntityResult());

            UpdateEntityRequest result = null;
            this.treeStoreService
                .Setup(s => s.UpdateEntityAsync(entity.Id, It.IsAny<UpdateEntityRequest>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, UpdateEntityRequest, CancellationToken>((_, updt, _) => result = updt)
                .ReturnsAsync(entity.ToEntityResult());

            var entityNodeAdapter = new EntityNodeAdapter(this.treeStoreService.Object, entity.Id);

            var value = Guid.NewGuid();
            entity.SetFacetProperty(category.FacetProperties().Single(), value);

            // ACT
            entityNodeAdapter.GetRequiredService<IClearItemProperty>().ClearItemProperty("guid".Yield());

            // ASSERT
            Assert.Null(result.Values.Updates.Single().Value);
        }

        #endregion IClearItemProperty
    }
}