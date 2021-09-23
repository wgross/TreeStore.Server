using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;
using TreeStore.Server.Client;
using Xunit;

namespace TreeStore.Server.Host.IntegTest
{
    public class CategoryTest
    {
        private readonly TreeStoreTestServer serverFactory;
        private readonly TreeStoreClient client;

        public CategoryTest()
        {
            this.serverFactory = new TreeStoreTestServer();
            this.client = new TreeStoreClient(this.serverFactory.CreateClient(), new NullLogger<TreeStoreClient>());
        }

        [Fact]
        public async Task Create_category()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            // ACT
            var request = new CreateCategoryRequest(
                Name: "c",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var result = await this.client.CreateCategoryAsync(request, CancellationToken.None);

            // ASSERT
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.ParentId, result.ParentId);

            FacetPropertyResult getResultProperty(string name) => result.Facet.Properties.Single(p => p.Name == name);

            Assert.All(request.Facet.Creates, c => Assert.Equal(c.Type, getResultProperty(c.Name).Type));
        }

        [Fact]
        public async Task Read_category_by_id()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            // ACT
            var request = new CreateCategoryRequest(
                Name: "c",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var category = await this.client.CreateCategoryAsync(request, CancellationToken.None);

            // ACT
            var result = await this.client.GetCategoryByIdAsync(category.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.ParentId, result.ParentId);

            FacetPropertyResult getResultProperty(Guid id) => result.Facet.Properties.Single(p => p.Id == id);

            Assert.All(category.Facet.Properties, p =>
            {
                Assert.Equal(p.Type, getResultProperty(p.Id).Type);
                Assert.Equal(p.Name, getResultProperty(p.Id).Name);
            });
        }

        [Fact]
        public async Task Read_child_categories_by_id()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            // ACT
            var request = new CreateCategoryRequest(
                Name: "c",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var category = await this.client.CreateCategoryAsync(request, CancellationToken.None);

            // ACT
            var result = await this.client.GetCategoriesByIdAsync(category.ParentId, CancellationToken.None);

            // ASSERT
            Assert.Equal(category.Id, result.Single().Id);
            Assert.Equal(category.Name, result.Single().Name);
            Assert.Equal(category.ParentId, result.Single().ParentId);

            FacetPropertyResult getResultProperty(Guid id) => result.Single().Facet.Properties.Single(p => p.Id == id);

            Assert.All(category.Facet.Properties, p =>
            {
                Assert.Equal(p.Type, getResultProperty(p.Id).Type);
                Assert.Equal(p.Name, getResultProperty(p.Id).Name);
            });
        }

        [Fact]
        public async Task Update_category_by_id()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            var createRequest = new CreateCategoryRequest(
                Name: "c",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var category = await this.client.CreateCategoryAsync(createRequest, CancellationToken.None);

            // ACT
            var updateRequest = new UpdateCategoryRequest(
                Name: "changed",
                Facet: new FacetRequest(
                    new UpdateFacetPropertyRequest(category.Facet.Properties.ElementAt(0).Id, Name: "changed"),
                    new DeleteFacetPropertyRequest(category.Facet.Properties.ElementAt(1).Id),
                    new CreateFacetPropertyRequest(Name: "p3", Type: FacetPropertyTypeValues.Bool)));

            var result = await this.client.UpdateCategoryAsync(category.Id, updateRequest, CancellationToken.None);

            // ASSERT
            Assert.Equal(category.Id, result.Id);
            Assert.Equal("changed", result.Name);
            Assert.Equal(category.ParentId, result.ParentId);

            FacetPropertyResult getResultProperty(Guid id) => result.Facet.Properties.Single(p => p.Id == id);

            Assert.Equal(new[] { "changed", "p3" }, result.Facet.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { FacetPropertyTypeValues.DateTime, FacetPropertyTypeValues.Bool }, result.Facet.Properties.Select(p => p.Type).ToArray());
        }

        [Fact]
        public async Task Delete_category_by_id()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            var createRequest = new CreateCategoryRequest(
                Name: "c",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var category = await this.client.CreateCategoryAsync(createRequest, CancellationToken.None);

            // ACT
            var result = await this.client.DeleteCategoryAsync(category.Id, recurse: false, CancellationToken.None);

            // ASSERT
            Assert.True(result);
            Assert.Null(await this.client.GetCategoryByIdAsync(category.Id, CancellationToken.None));
        }

        [Fact]
        public async Task Delete_category_by_childname()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            var createRequest = new CreateCategoryRequest(
                Name: "c",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var category = await this.client.CreateCategoryAsync(createRequest, CancellationToken.None);

            // ACT
            var result = await this.client.DeleteCategoryAsync(rootCategory.Id, category.Name, recurse: false, CancellationToken.None);

            // ASSERT
            Assert.True(result);
            Assert.Null(await this.client.GetCategoryByIdAsync(category.Id, CancellationToken.None));
        }
    }
}