using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using TreeStore.Server.Client;
using TreeStore.Server.Host.Controllers;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.Test.Controllers
{
    public class CategoryControllerTest : TreeStoreServerHostTestBase
    {
        private readonly Category rootCategory;
        private readonly IHost host;
        private readonly Mock<ITreeStoreService> serviceMock;
        private CancellationTokenSource cancellationTokenSource;
        private TreeStoreClient service;

        public CategoryControllerTest()
        {
            // model
            this.rootCategory = DefaultRootCategory();

            // server
            this.serviceMock = this.Mocks.Create<ITreeStoreService>();
            this.host = Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder()
                .ConfigureWebHost(wh =>
                {
                    wh.UseTestServer();
                    wh.UseStartup(whctx => new TestStartup(this.serviceMock.Object, whctx.Configuration));
                })
                .Build();
            this.host.StartAsync();

            // client
            this.cancellationTokenSource = new CancellationTokenSource();
            this.service = new TreeStoreClient(this.host.GetTestClient(), new NullLogger<TreeStoreClient>());
        }

        #region CREATE

        [Fact]
        public async Task Create_new_category()
        {
            // ARRANGE
            var category = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.CreateCategoryAsync(It.Is<CreateCategoryRequest>(r => category.Name.Equals(r.Name)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToDto());

            // ACT
            var result = await this.service.CreateCategoryAsync(new(category.Name, this.rootCategory.Id), CancellationToken.None);
        }

        #endregion CREATE

        #region READ

        [Fact]
        public async Task Read_category_by_id()
        {
            // ARRANGE
            var category = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToDto());

            // ACT
            var result = await this.service.GetCategoryByIdAsync(category.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(category.ToDto(), result);
        }

        [Fact]
        public async Task Reading_unknown_category_by_id_returns_null()
        {
            // ARRANGE
            this.serviceMock
                .Setup(s => s.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CategoryResponse)null);

            // ACT
            var result = await this.service.GetCategoryByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        #endregion READ

        #region UPDATE

        [Fact]
        public async Task Update_category()
        {
            // ARRANGE
            var categeory = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.UpdateCategoryAsync(categeory.Id, It.IsAny<UpdateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(categeory.ToDto());

            // ACT
            var result = await this.service.UpdateCategoryAsync(categeory.Id, new UpdateCategoryRequest(categeory.Name), CancellationToken.None);

            // ASSERT
            Assert.Equal(categeory.ToDto(), result);
        }

        [Fact]
        public async Task Update_rethrows_if_update_fails()
        {
            // ARRANGE
            var entity = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.UpdateCategoryAsync(entity.Id, It.IsAny<UpdateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("fail"));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service.UpdateCategoryAsync(entity.Id, new UpdateCategoryRequest(entity.Name), CancellationToken.None));

            // ASSERT
            Assert.Equal("fail", result.Message);
        }

        #endregion UPDATE

        #region DELETE

        [Fact]
        public async Task Delete_category()
        {
            // ARRANGE
            var entity = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.DeleteEntityAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteEntityResponse(true));

            // ACT
            var result = await this.service.DeleteEntityAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.True(result.Deleted);
        }

        #endregion DELETE
    }
}