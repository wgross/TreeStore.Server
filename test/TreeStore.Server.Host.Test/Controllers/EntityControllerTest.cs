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
            var entity = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues);

            CreateEntityRequest writtenEntity = null;
            this.modelServiceMock
                .Setup(s => s.CreateEntityAsync(It.IsAny<CreateEntityRequest>(), It.IsAny<CancellationToken>()))
                .Callback<CreateEntityRequest, CancellationToken>((r, _) => writtenEntity = r)
                .ReturnsAsync(entity.ToEntityResult());

            var request = new CreateEntityRequest(
                    Name: entity.Name,
                    CategoryId: entity.Category.Id,
                    Tags: new CreateEntityTagsRequest(new AssignTagRequest(entity.Tags.Single().Id)),
                    Values: new FacetPropertyValuesRequest(
                        entity.FacetPropertyValues().Select(fpv => new UpdateFacetPropertyValueRequest(fpv.facetProperty.Id, fpv.facetProperty.Type, fpv.value)).ToArray()));

            // ACT
            var result = await this.clientService.CreateEntityAsync(createEntityRequest: request, cancellationToken: CancellationToken.None);

            // ASSERT
            var expected = entity.ToEntityResult();

            FacetPropertyValueResult resultValue(Guid id) => result.Values.First(v => v.Id == id);

            Assert.Equal(expected.Id, result.Id);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.TagIds.Single(), result.TagIds.Single());
            Assert.All(expected.Values, ev =>
            {
                Assert.Equal(ev.Value, resultValue(ev.Id).Value);
                Assert.Equal(ev.Type, resultValue(ev.Id).Type);
            });

            UpdateFacetPropertyValueRequest writtenValue(Guid id) => writtenEntity.Values.Updates.First(u => u.Id == id);

            Assert.Equal(expected.Name, writtenEntity.Name);
            Assert.Equal(expected.TagIds.Single(), writtenEntity.Tags.Assigns.Select(c => c.TagId).Single());
            Assert.All(expected.Values, ev =>
            {
                Assert.Equal(ev.Value, writtenValue(ev.Id).Value);
                Assert.Equal(ev.Type, writtenValue(ev.Id).Type);
            });
        }

        [Fact]
        public async Task Creating_entity_rethrows()
        {
            // ARRANGE
            var entity = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues);

            this.modelServiceMock
                .Setup(s => s.CreateEntityAsync(It.IsAny<CreateEntityRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidModelException("fail", new Exception("innerFail")));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidModelException>(() => this.clientService.CreateEntityAsync(new CreateEntityRequest(entity.Name, entity.Category.Id), CancellationToken.None));

            // ASSERT
            Assert.Equal("fail: innerFail", result.Message);
        }

        #endregion CREATE

        #region READ

        [Fact]
        public async Task Read_entity_by_id()
        {
            // ARRANGE
            var entity = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues);

            this.modelServiceMock
                .Setup(s => s.GetEntityByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToEntityResult());

            // ACT
            var result = await this.clientService.GetEntityByIdAsync(entity.Id, CancellationToken.None);

            // ASSERT
            var expected = entity.ToEntityResult();

            FacetPropertyValueResult resultValue(Guid id) => result.Values.First(v => v.Id == id);

            Assert.Equal(expected.Id, result.Id);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.TagIds.Single(), result.TagIds.Single());
            Assert.All(expected.Values, ev =>
            {
                Assert.Equal(ev.Value, resultValue(ev.Id).Value);
                Assert.Equal(ev.Type, resultValue(ev.Id).Type);
            });
        }

        [Fact]
        public async Task Reading_unknown_entity_by_id_returns_null()
        {
            // ARRANGE
            this.modelServiceMock
                .Setup(s => s.GetEntityByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityResult)null);

            // ACT
            var result = await this.clientService.GetEntityByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Read_all_entities()
        {
            // ARRANGE
            var entity = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues);

            this.modelServiceMock
                .Setup(s => s.GetEntitiesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { entity.ToEntityResult() });

            // ACT
            var result = await this.clientService.GetEntitiesAsync(CancellationToken.None);

            // ASSERT
            var expected = entity.ToEntityResult();

            Assert.Equal(expected.Id, result.Single().Id);
            Assert.Equal(expected.Name, result.Single().Name);
            Assert.Equal(expected.TagIds.Single(), result.Single().TagIds.Single());
        }

        #endregion READ

        #region UPDATE

        [Fact]
        public async Task Update_entity()
        {
            // ARRANGE
            var entity = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues);

            UpdateEntityRequest writtenEntity = null;
            this.modelServiceMock
                .Setup(s => s.UpdateEntityAsync(entity.Id, It.IsAny<UpdateEntityRequest>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, UpdateEntityRequest, CancellationToken>((_1, updt, _2) => writtenEntity = updt)
                .ReturnsAsync(entity.ToEntityResult());

            var request = new UpdateEntityRequest(
                  Name: entity.Name,
                  Tags: new UpdateEntityTagsRequest(new[]
                  {
                        new AssignTagRequest(TagId:entity.Tags.Single().Id)
                  }),
                  Values: new FacetPropertyValuesRequest(entity
                      .FacetPropertyValues()
                      .Select(fpv => new UpdateFacetPropertyValueRequest(fpv.facetProperty.Id, fpv.facetProperty.Type, fpv.value))
                      .ToArray()));

            // ACT
            var result = await this.clientService.UpdateEntityAsync(entity.Id, request, CancellationToken.None);

            // ASSERT
            var expected = entity.ToEntityResult();

            FacetPropertyValueResult resultValue(Guid id) => result.Values.First(v => v.Id == id);

            Assert.Equal(expected.Id, result.Id);
            Assert.Equal(expected.Name, result.Name);
            Assert.Equal(expected.TagIds.Single(), result.TagIds.Single());
            Assert.All(expected.Values, ev =>
            {
                Assert.Equal(ev.Value, resultValue(ev.Id).Value);
                Assert.Equal(ev.Type, resultValue(ev.Id).Type);
            });

            UpdateFacetPropertyValueRequest writtenValue(Guid id) => writtenEntity.Values.Updates.First(u => u.Id == id);

            Assert.Equal(expected.Name, writtenEntity.Name);
            Assert.Equal(expected.TagIds.Single(), writtenEntity.Tags.Assigns.Select(c => c.TagId).Single());
            Assert.All(expected.Values, ev =>
            {
                Assert.Equal(ev.Value, writtenValue(ev.Id).Value);
                Assert.Equal(ev.Type, writtenValue(ev.Id).Type);
            });
        }

        [Fact]
        public async Task Updating_entity_fails()
        {
            // ARRANGE
            var entity = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues);

            this.modelServiceMock
                .Setup(s => s.UpdateEntityAsync(entity.Id, It.IsAny<UpdateEntityRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidModelException("fail", new Exception("innerFail")));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidModelException>(() => this.clientService.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(entity.Name), CancellationToken.None));

            // ASSERT
            Assert.Equal("fail: innerFail", result.Message);
        }

        #endregion UPDATE

        #region DELETE

        [Fact]
        public async Task Delete_entity()
        {
            // ARRANGE
            var entity = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues);

            this.modelServiceMock
                .Setup(s => s.DeleteEntityAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await this.clientService.DeleteEntityAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.True(result);
        }

        #endregion DELETE
    }
}