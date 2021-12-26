using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;
using TreeStore.Server.Client;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.IntegTest
{
    public class EntityTest
    {
        private readonly TreeStoreTestServer serverFactory;
        private readonly TreeStoreClient client;

        public EntityTest()
        {
            this.serverFactory = new TreeStoreTestServer();
            this.client = new TreeStoreClient(this.serverFactory.CreateClient(), new NullLogger<TreeStoreClient>());
        }

        #region CREATE / COPY / MOVE

        [Fact]
        public async Task Create_entity()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);
            var category = await this.client.UpdateCategoryAsync(rootCategory.Id,
                 request: new UpdateCategoryRequest(
                    Facet: new FacetRequest(new CreateFacetPropertyRequest("p1", FacetPropertyTypeValues.String))),
                cancellationToken: CancellationToken.None).ConfigureAwait(false);

            var entity = DefaultEntityModel();

            // ACT
            var request = new CreateEntityRequest(
                Name: entity.Name,
                CategoryId: rootCategory.Id,
                Values: new FacetPropertyValuesRequest(new UpdateFacetPropertyValueRequest(category.Facet.Properties.Single().Id, category.Facet.Properties.Single().Type, "value")));

            var result = await this.client.CreateEntityAsync(request, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(rootCategory.Id, result.CategoryId);
            Assert.Equal(category.Facet.Properties.Single().Id, result.Values.Single().Id);
            Assert.Equal(category.Facet.Properties.Single().Type, result.Values.Single().Type);
            Assert.Null(result.Values.Single().Value);
        }

        [Fact]
        public async Task Copy_entity()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            var createC1 = new CreateEntityRequest(
                Name: "c1",
                CategoryId: rootCategory.Id);
            var c1 = await this.client.CreateEntityAsync(createC1, CancellationToken.None).ConfigureAwait(false);

            var createC2 = new CreateCategoryRequest(
                Name: "c2",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));
            var c2 = await this.client.CreateCategoryAsync(createC2, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var result = await this.client.CopyEntityToAsync(c1.Id, c2.Id, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(c1.Name, result.Name);
            Assert.Equal(c2.Id, result.CategoryId);
            Assert.NotEqual(c1.Id, result.Id);
        }

        [Fact]
        public async Task Move_entity()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            var createC1 = new CreateEntityRequest(
                Name: "c1",
                CategoryId: rootCategory.Id);
            var c1 = await this.client.CreateEntityAsync(createC1, CancellationToken.None).ConfigureAwait(false);

            var createC2 = new CreateCategoryRequest(
                Name: "c2",
                ParentId: rootCategory.Id);
            var c2 = await this.client.CreateCategoryAsync(createC2, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var result = await this.client.MoveEntityToAsync(c1.Id, c2.Id, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(c1.Name, result.Name);
            Assert.Equal(c2.Id, result.CategoryId);
            Assert.Equal(c1.Id, result.Id);
        }

        #endregion CREATE / COPY / MOVE

        [Fact]
        public async Task Update_entity_values()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);
            var category = await this.client.UpdateCategoryAsync(rootCategory.Id,
                 request: new UpdateCategoryRequest(
                    Facet: new FacetRequest(new CreateFacetPropertyRequest("p1", FacetPropertyTypeValues.String))),
                cancellationToken: CancellationToken.None).ConfigureAwait(false);

            var entityModel = DefaultEntityModel();

            var createRequest = new CreateEntityRequest(
                Name: entityModel.Name,
                CategoryId: rootCategory.Id,
                Values: new FacetPropertyValuesRequest(new UpdateFacetPropertyValueRequest(category.Facet.Properties.Single().Id, category.Facet.Properties.Single().Type, "value")));

            var entityResult = await this.client.CreateEntityAsync(createRequest, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var request = new UpdateEntityRequest(
                Values: new FacetPropertyValuesRequest(
                    new UpdateFacetPropertyValueRequest(category.Facet.Properties.Single().Id, category.Facet.Properties.Single().Type, "changed-value")));

            var result = await this.client.UpdateEntityAsync(entityResult.Id, request, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(entityModel.Name, result.Name);
            Assert.Equal(rootCategory.Id, result.CategoryId);
            Assert.Equal(category.Facet.Properties.Single().Id, result.Values.Single().Id);
            Assert.Equal(category.Facet.Properties.Single().Type, result.Values.Single().Type);
            Assert.Equal("changed-value", result.Values.Single().Value);
        }

        [Fact]
        public async Task Delete_entity_by_id()
        {
            // ARRANGE
            // put entity under root category
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);
            var entity = DefaultEntityModel();

            var request = new CreateEntityRequest(
                Name: entity.Name,
                CategoryId: rootCategory.Id);

            var entityResult = await this.client.CreateEntityAsync(request, CancellationToken.None).ConfigureAwait(false);

            // ACT
            // delete the entity with the id from the create response.
            var result = await this.client.DeleteEntityAsync(entityResult.Id, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public async Task Read_entity_by_id()
        {
            // ARRANGE
            // create a facet at the root category
            var rootCategory = await this.client.UpdateCategoryAsync(
                id: (await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false)).Id,
                request: new UpdateCategoryRequest(Facet: new FacetRequest(new CreateFacetPropertyRequest("p1", FacetPropertyTypeValues.String))),
                cancellationToken: CancellationToken.None).ConfigureAwait(false);

            // put entity under root category with facet property value
            var entity = DefaultEntityModel();

            var request = new CreateEntityRequest(
              Name: entity.Name,
              CategoryId: rootCategory.Id,
              Values: new FacetPropertyValuesRequest(new UpdateFacetPropertyValueRequest(rootCategory.Facet.Properties.Single().Id, rootCategory.Facet.Properties.Single().Type, "value")));

            var entityResult = await this.client.CreateEntityAsync(request, CancellationToken.None).ConfigureAwait(false);

            // ACT
            // read entity using the id from create request
            var result = await this.client.GetEntityByIdAsync(entityResult.Id, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(rootCategory.Id, result.CategoryId);
            Assert.Equal(rootCategory.Facet.Properties.Single().Id, result.Values.Single().Id);
            Assert.Equal(rootCategory.Facet.Properties.Single().Type, result.Values.Single().Type);
            Assert.Null(result.Values.Single().Value);
        }
    }
}