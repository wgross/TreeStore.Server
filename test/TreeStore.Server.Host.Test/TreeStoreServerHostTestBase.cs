using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using TreeStore.Model.Abstractions;
using TreeStore.Server.Client;

namespace TreeStore.Server.Host.Test
{
    public abstract class TreeStoreServerHostTestBase : IDisposable
    {
        private bool disposedValue;
        protected IHost Host { get; }
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected Mock<ITreeStoreService> ModelServiceMock { get; }

        protected TreeStoreClient clientService;

        public TreeStoreServerHostTestBase()
        {
            // server
            this.ModelServiceMock = this.Mocks.Create<ITreeStoreService>();

            this.Host = Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder()
                .ConfigureWebHost(wh =>
                {
                    wh.UseTestServer();
                    wh.UseStartup(whctx => new TestStartup(this.ModelServiceMock.Object, whctx.Configuration));
                })
                .Build();
            this.Host.StartAsync();

            // client
            this.clientService = new TreeStoreClient(this.Host.GetTestClient(), new NullLogger<TreeStoreClient>());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Host.StopAsync().GetAwaiter().GetResult();
                    this.Mocks.VerifyAll();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}