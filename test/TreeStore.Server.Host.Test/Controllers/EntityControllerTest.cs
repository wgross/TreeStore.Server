using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
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
    public class EntityControllerTest : TreeStoreServerHostTestBase
    {
        private readonly Mock<ITreeStoreService> serviceMock;
        private readonly IHost host;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly TreeStoreClient service;
        private readonly Category rootCategory;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                this.host.StopAsync().GetAwaiter().GetResult();
            base.Dispose(disposing);
        }

        public EntityControllerTest()
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
        public async Task Create_new_entity()
        {
            // ARRANGE
            var entity = DefaultEntity(this.rootCategory);

            this.serviceMock
                .Setup(s => s.CreateEntityAsync(It.Is<CreateEntityRequest>(r => entity.Name.Equals(r.Name)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToDto());

            // ACT
            var result = await this.service.CreateEntityAsync(new CreateEntityRequest(entity.Name), CancellationToken.None);
        }

        #endregion CREATE

        #region READ

        [Fact]
        public async Task Read_entity_by_id()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultCategory(this.rootCategory));

            this.serviceMock
                .Setup(s => s.GetEntityByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToDto());

            // ACT
            var result = await this.service.GetEntityByIdAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.ToDto(), result);
        }

        [Fact]
        public async Task Reading_unknown_entity_by_id_returns_null()
        {
            // ARRANGE
            this.serviceMock
                .Setup(s => s.GetEntityByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EntityResponse)null);

            // ACT
            var result = await this.service.GetEntityByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Read_all_entities()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultCategory(this.rootCategory));

            this.serviceMock
                .Setup(s => s.GetEntitiesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { entity.ToDto() });

            // ACT
            var result = await this.service.GetEntitiesAsync(CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.ToDto(), result.Single());
        }

        #endregion READ

        #region UPDATE

        [Fact]
        public async Task Update_entity()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultCategory(this.rootCategory));

            this.serviceMock
                .Setup(s => s.UpdateEntityAsync(entity.Id, It.IsAny<UpdateEntityRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToDto());

            // ACT
            var result = await this.service.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(entity.Name), CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.ToDto(), result);
        }

        [Fact]
        public async Task Update_rethrows_if_update_fails()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultCategory(this.rootCategory));

            this.serviceMock
                .Setup(s => s.UpdateEntityAsync(entity.Id, It.IsAny<UpdateEntityRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("fail"));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(entity.Name), CancellationToken.None));

            // ASSERT
            Assert.Equal("fail", result.Message);
        }

        #endregion UPDATE

        #region DELETE

        [Fact]
        public async Task Delete_entity()
        {
            // ARRANGE
            var entity = DefaultEntity(DefaultCategory(this.rootCategory));

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