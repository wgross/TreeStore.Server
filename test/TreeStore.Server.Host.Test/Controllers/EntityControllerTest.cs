using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.Test.Controllers
{
    public class EntityControllerTest : TreeStoreServerHostTestBase
    {
        private readonly CategoryModel rootCategory;

        public EntityControllerTest()
        {
            // model
            this.rootCategory = DefaultRootCategoryModel();
        }

        #region CREATE

        [Fact]
        public async Task Create_new_entity()
        {
            // ARRANGE
            var entity = DefaultEntityModel(this.rootCategory);

            this.serviceMock
                .Setup(s => s.CreateEntityAsync(It.Is<CreateEntityRequest>(r => entity.Name.Equals(r.Name) && entity.Category.Id.Equals(r.CategoryId)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToEntityResult());

            // ACT
            var result = await this.service.CreateEntityAsync(new CreateEntityRequest(entity.Name, entity.Category.Id), CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.ToEntityResult(), result);
        }

        [Fact]
        public async Task Creating_entity_rethrows()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultCategoryModel(this.rootCategory));

            this.serviceMock
                .Setup(s => s.CreateEntityAsync(It.IsAny<CreateEntityRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidModelException("fail", new Exception("innerFail")));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidModelException>(() => this.service.CreateEntityAsync(new CreateEntityRequest(entity.Name, entity.Category.Id), CancellationToken.None));

            // ASSERT
            Assert.Equal("fail: innerFail", result.Message);
        }

        #endregion CREATE

        #region READ

        [Fact]
        public async Task Read_entity_by_id()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultCategoryModel(this.rootCategory));

            this.serviceMock
                .Setup(s => s.GetEntityByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToEntityResult());

            // ACT
            var result = await this.service.GetEntityByIdAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.ToEntityResult(), result);
        }

        [Fact]
        public async Task Reading_unknown_entity_by_id_returns_null()
        {
            // ARRANGE
            this.serviceMock
                .Setup(s => s.GetEntityByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityResult)null);

            // ACT
            var result = await this.service.GetEntityByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Read_all_entities()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultCategoryModel(this.rootCategory));

            this.serviceMock
                .Setup(s => s.GetEntitiesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { entity.ToEntityResult() });

            // ACT
            var result = await this.service.GetEntitiesAsync(CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.ToEntityResult(), result.Single());
        }

        #endregion READ

        #region UPDATE

        [Fact]
        public async Task Update_entity()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultCategoryModel(this.rootCategory));

            this.serviceMock
                .Setup(s => s.UpdateEntityAsync(entity.Id, It.IsAny<UpdateEntityRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToEntityResult());

            // ACT
            var result = await this.service.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(entity.Name), CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.ToEntityResult(), result);
        }

        [Fact]
        public async Task Updating_entity_fails()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultCategoryModel(this.rootCategory));

            this.serviceMock
                .Setup(s => s.UpdateEntityAsync(entity.Id, It.IsAny<UpdateEntityRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidModelException("fail", new Exception("innerFail")));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidModelException>(() => this.service.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(entity.Name), CancellationToken.None));

            // ASSERT
            Assert.Equal("fail: innerFail", result.Message);
        }

        #endregion UPDATE

        #region DELETE

        [Fact]
        public async Task Delete_entity()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultCategoryModel(this.rootCategory));

            this.serviceMock
                .Setup(s => s.DeleteEntityAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await this.service.DeleteEntityAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.True(result);
        }

        #endregion DELETE
    }
}