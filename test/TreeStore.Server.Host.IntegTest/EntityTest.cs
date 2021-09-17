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

        [Fact]
        public async Task Create_entity()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None);
            var category = await this.client.UpdateCategoryAsync(rootCategory.Id,
                 request: new UpdateCategoryRequest(
                    Facet: new FacetRequest(new CreateFacetPropertyRequest("p1", FacetPropertyTypeValues.String))),
                cancellationToken: CancellationToken.None);

            var entity = DefaultEntityModel();

            // ACT
            var request = new CreateEntityRequest(
                Name: entity.Name,
                CategoryId: rootCategory.Id,
                Values: new FacetPropertyValuesRequest(new UpdateFacetPropertyValueRequest(category.Facet.Properties.Single().Id, category.Facet.Properties.Single().Type, "value")));

            var result = await this.client.CreateEntityAsync(request, CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(rootCategory.Id, result.CategoryId);
            Assert.Equal(category.Facet.Properties.Single().Id, result.Values.Single().Id);
            Assert.Equal(category.Facet.Properties.Single().Type, result.Values.Single().Type);
            Assert.Null(result.Values.Single().Value);
        }

        [Fact]
        public async Task Update_entity_values()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None);
            var category = await this.client.UpdateCategoryAsync(rootCategory.Id,
                 request: new UpdateCategoryRequest(
                    Facet: new FacetRequest(new CreateFacetPropertyRequest("p1", FacetPropertyTypeValues.String))),
                cancellationToken: CancellationToken.None);

            var entityModel = DefaultEntityModel();

            var createRequest = new CreateEntityRequest(
                Name: entityModel.Name,
                CategoryId: rootCategory.Id,
                Values: new FacetPropertyValuesRequest(new UpdateFacetPropertyValueRequest(category.Facet.Properties.Single().Id, category.Facet.Properties.Single().Type, "value")));

            var entityResult = await this.client.CreateEntityAsync(createRequest, CancellationToken.None);

            // ACT
            var request = new UpdateEntityRequest(
                Values: new FacetPropertyValuesRequest(
                    new UpdateFacetPropertyValueRequest(category.Facet.Properties.Single().Id, category.Facet.Properties.Single().Type, "changed-value")));

            var result = await this.client.UpdateEntityAsync(entityResult.Id, request, CancellationToken.None);

            // ASSERT
            Assert.Equal(entityModel.Name, result.Name);
            Assert.Equal(rootCategory.Id, result.CategoryId);
            Assert.Equal(category.Facet.Properties.Single().Id, result.Values.Single().Id);
            Assert.Equal(category.Facet.Properties.Single().Type, result.Values.Single().Type);
            Assert.Equal("changed-value", result.Values.Single().Value);
        }

        [Fact]
        public async Task Read_entities_by_category()
        {
        }
    }
}