using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Threading;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using TreeStore.Server.Client;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.Test.Controllers
{
    public partial class CategoryControllerTest : TreeStoreServerHostTestBase
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
    }
}