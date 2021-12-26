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

        #region CREATE / COPY / MOVE

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

            var result = await this.client.CreateCategoryAsync(request, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.ParentId, result.ParentId);

            FacetPropertyResult getResultProperty(string name) => result.Facet.Properties.Single(p => p.Name == name);

            Assert.All(request.Facet.Creates, c => Assert.Equal(c.Type, getResultProperty(c.Name).Type));
        }

        [Fact]
        public async Task Copy_category()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            var createC1 = new CreateCategoryRequest(
                Name: "c1",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var c1 = await this.client.CreateCategoryAsync(createC1, CancellationToken.None).ConfigureAwait(false);

            var createC2 = new CreateCategoryRequest(
                Name: "c2",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var c2 = await this.client.CreateCategoryAsync(createC2, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var result = await this.client.CopyCategoryToAsync(c1.Id, c2.Id, false, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(c1.Name, result.Name);
            Assert.Equal(c2.Id, result.ParentId);
            Assert.NotEqual(c1.Id, result.Id);
        }

        [Fact]
        public async Task Move_category()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            var createC1 = new CreateCategoryRequest(
                Name: "c1",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var c1 = await this.client.CreateCategoryAsync(createC1, CancellationToken.None).ConfigureAwait(false);

            var createC2 = new CreateCategoryRequest(
                Name: "c2",
                ParentId: rootCategory.Id,
                Facet: new FacetRequest(
                        new CreateFacetPropertyRequest(Name: "p1", Type: FacetPropertyTypeValues.DateTime),
                        new CreateFacetPropertyRequest(Name: "p2", Type: FacetPropertyTypeValues.Long)));

            var c2 = await this.client.CreateCategoryAsync(createC2, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var result = await this.client.MoveCategoryToAsync(c1.Id, c2.Id, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(c1.Name, result.Name);
            Assert.Equal(c2.Id, result.ParentId);
            Assert.Equal(c1.Id, result.Id);
        }

        #endregion CREATE / COPY / MOVE

        #region READ

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

            var category = await this.client.CreateCategoryAsync(request, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var result = await this.client.GetCategoryByIdAsync(category.Id, CancellationToken.None).ConfigureAwait(false);

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

            var category = await this.client.CreateCategoryAsync(request, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var result = await this.client.GetCategoriesByIdAsync(category.ParentId, CancellationToken.None).ConfigureAwait(false);

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

        #endregion READ

        #region UPDATE

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

            var category = await this.client.CreateCategoryAsync(createRequest, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var updateRequest = new UpdateCategoryRequest(
                Name: "changed",
                Facet: new FacetRequest(
                    new UpdateFacetPropertyRequest(category.Facet.Properties.ElementAt(0).Id, Name: "changed"),
                    new DeleteFacetPropertyRequest(category.Facet.Properties.ElementAt(1).Id),
                    new CreateFacetPropertyRequest(Name: "p3", Type: FacetPropertyTypeValues.Bool)));

            var result = await this.client.UpdateCategoryAsync(category.Id, updateRequest, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Equal(category.Id, result.Id);
            Assert.Equal("changed", result.Name);
            Assert.Equal(category.ParentId, result.ParentId);

            Assert.Equal(new[] { "changed", "p3" }, result.Facet.Properties.Select(p => p.Name).ToArray());
            Assert.Equal(new[] { FacetPropertyTypeValues.DateTime, FacetPropertyTypeValues.Bool }, result.Facet.Properties.Select(p => p.Type).ToArray());
        }

        [Fact]
        public async Task Update_category_name_by_id_fails_on_duplicate_category_name()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            var createRequest1 = new CreateCategoryRequest(
                Name: "c1",
                ParentId: rootCategory.Id);

            var category1 = await this.client.CreateCategoryAsync(createRequest1, CancellationToken.None).ConfigureAwait(false);

            var createRequest2 = new CreateCategoryRequest(
                Name: "c2",
                ParentId: rootCategory.Id);

            var category2 = await this.client.CreateCategoryAsync(createRequest2, CancellationToken.None).ConfigureAwait(false);

            // ACT
            // give category1 the name of category2
            var updateRequest = new UpdateCategoryRequest(Name: "c2");

            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.client.UpdateCategoryAsync(category1.Id, updateRequest, CancellationToken.None)).ConfigureAwait(false);

            // ASSERT
            Assert.StartsWith("Can't write Category(name='c2'): duplicate name:", result.Message);
        }

        [Fact]
        public async Task Update_category_name_by_id_fails_on_duplicate_entity_name()
        {
            // ARRANGE
            var rootCategory = await this.client.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

            var createRequest1 = new CreateCategoryRequest(
                Name: "c1",
                ParentId: rootCategory.Id);

            var category1 = await this.client.CreateCategoryAsync(createRequest1, CancellationToken.None).ConfigureAwait(false);

            var createRequest2 = new CreateEntityRequest(
                Name: "c2",
                CategoryId: rootCategory.Id);

            var entity = await this.client.CreateEntityAsync(createRequest2, CancellationToken.None).ConfigureAwait(false);

            // ACT
            // give category1 the name of category2
            var updateRequest = new UpdateCategoryRequest(Name: "c2");

            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.client.UpdateCategoryAsync(category1.Id, updateRequest, CancellationToken.None)).ConfigureAwait(false);

            // ASSERT
            Assert.StartsWith($"Category(id='{category1.Id}') wasn't updated: duplicate name with Entity(id='{entity.Id}')", result.Message);
        }

        #endregion UPDATE

        #region DELETE

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

            var category = await this.client.CreateCategoryAsync(createRequest, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var result = await this.client.DeleteCategoryAsync(category.Id, recurse: false, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.True(result);
            Assert.Null(await this.client.GetCategoryByIdAsync(category.Id, CancellationToken.None).ConfigureAwait(false));
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

            var category = await this.client.CreateCategoryAsync(createRequest, CancellationToken.None).ConfigureAwait(false);

            // ACT
            var result = await this.client.DeleteCategoryAsync(rootCategory.Id, category.Name, recurse: false, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.True(result);
            Assert.Null(await this.client.GetCategoryByIdAsync(category.Id, CancellationToken.None).ConfigureAwait(false));
        }

        #endregion DELETE
    }
}